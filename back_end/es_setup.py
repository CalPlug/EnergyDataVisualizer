import json
import logging
import sys
import os
import base64
import datetime
import hashlib
import hmac 
import requests
import urllib.parse
import logging
import pprint

pp = pprint.PrettyPrinter(indent=4)

logging.basicConfig(format='%(asctime)s %(message)s', level=logging.INFO)

# Load and dump json just to be safe. Sometimes when copying and pasting payloads
# characters that look like whitespace are copied in as well, and can mess up sigV4
# payload calculation
MAPPING = json.dumps(json.loads("""
{
    "mappings": {
        "sensordatamodel": { 
            "properties": {
                "uid":    { "type": "keyword"  }, 
                "timestamp" : { "type" : "date" },
                "PowerFactor" : { "type": "integer"},
                "CurrentSummationDelivered" : { "type" : "integer"},
                "InstantaneousDemand" : { "type": "integer"},
                "Current" : { "type": "integer"}
            }
        }
    }
}
"""))
ALIAS = json.dumps(json.loads("""
{
    "actions" : [
        { "add" : { "index" : "sensor-data-v1", "alias" : "sensor-data" } }
    ]
}
"""))


# Key derivation functions. See:
# http://docs.aws.amazon.com/general/latest/gr/signature-v4-examples.html#signature-v4-examples-python
def sign(key, msg):
    return hmac.new(key, msg.encode('utf-8'), hashlib.sha256).digest()

def getSignatureKey(key, dateStamp, regionName, serviceName):
    kDate = sign(('AWS4' + key).encode('utf-8'), dateStamp)
    kRegion = sign(kDate, regionName)
    kService = sign(kRegion, serviceName)
    kSigning = sign(kService, 'aws4_request')
    return kSigning

# See the url below for an explanation of the AWSrequest function
# http://docs.aws.amazon.com/general/latest/gr/sigv4-create-canonical-request.html 
def AWSrequest(method, url, payload, config):
    t = datetime.datetime.utcnow()
    amzdate = t.strftime('%Y%m%dT%H%M%SZ')
    datestamp = t.strftime('%Y%m%d') # Date w/o time, used in credential scope

    # Separate URL into various parts
    urlObj = urllib.parse.urlparse(url)
    HOST = urlObj.netloc
    canonical_uri = urlObj.path
    canonical_querystring = urlObj.query

    # Unpack config
    ACCESS_KEY = config['ACCESS_KEY']
    SECRET_KEY = config['SECRET_KEY']
    REGION = config['REGION']
    SERVICE = config['SERVICE']

    # Generate Canonical Request
    canonical_headers = 'host:{}\nx-amz-date:{}\n'.format(HOST, amzdate)
    SIGNED_HEADERS = 'host;x-amz-date'
    payload_hash = hashlib.sha256(payload.encode('utf-8')).hexdigest()
    canonical_request = '\n'.join([method, canonical_uri, canonical_querystring, 
                                    canonical_headers, SIGNED_HEADERS, payload_hash])
    logging.debug("Canonical request:\n" + canonical_request)

    # Generate signature key
    ALGORITHM = 'AWS4-HMAC-SHA256'
    credential_scope = '{}/{}/{}/aws4_request'.format(datestamp, REGION, SERVICE)
    string_to_sign = '\n'.join([ALGORITHM, amzdate, credential_scope,
                        hashlib.sha256(canonical_request.encode('utf-8')).hexdigest()])
    signing_key = getSignatureKey(SECRET_KEY, datestamp, REGION, SERVICE)
    signature = hmac.new(signing_key, (string_to_sign).encode('utf-8'), hashlib.sha256).hexdigest()
    logging.debug("Calculated signature: " + signature)
    
    authorization_header = '{} Credential={}/{},SignedHeaders={},Signature={}'.format(
        ALGORITHM, ACCESS_KEY, credential_scope, SIGNED_HEADERS, signature)
    headers = {
        'x-amz-date':amzdate,
        'Authorization':authorization_header
    }
    if payload:
        try:
            payload_json = json.loads(payload)
            return requests.request(method, url, headers=headers, json=payload_json)
        except:
            logging.warn("Skipping payload data. Invalid JSON payload: " + payload)
    return requests.request(method, url, headers=headers)    


if __name__ == '__main__':
    config = dict()
    config['ACCESS_KEY'] = input('Please enter your access key: ')
    config['SECRET_KEY'] = input('Please enter your secret key: ')
    config['REGION'] = input('Please enter your AWS region: ')
    config['SERVICE'] = 'es'
    ENDPOINT = input('Please enter in your ElasticEndpoint: ')
    
    mappingResponse = AWSrequest('PUT', urllib.parse.urljoin(ENDPOINT, 'sensor-data-v1'), MAPPING, config)
    logging.info('Mapping response:\n' + pp.pformat(mappingResponse.json()))
    mappingResponse.raise_for_status()

    aliasResponse = AWSrequest('POST', urllib.parse.urljoin(ENDPOINT, '_aliases'), ALIAS, config)
    logging.info('Alias response: ' + pp.pformat(aliasResponse.json()))
    aliasResponse.raise_for_status()
