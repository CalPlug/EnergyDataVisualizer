using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using CoreImage;
using Foundation;
using Newtonsoft.Json;
using Percept.Utility;
using UIKit;

namespace Percept.Model
{
    // class to access data from the aws backend
    public class AWSRestStore : IPersistentDataManager
    {
        // how often should cached images be replaced
        protected TimeSpan imageUpdateDelta;
        public TimeSpan ImageUpdateDelta
        {
            get => imageUpdateDelta;
            set
            {
                if (value.CompareTo(TimeSpan.Zero) == -1)
                {
                    throw new ArgumentException("Can't have a negative timespan");
                }
                else
                {
                    imageUpdateDelta = value;
                }
            }
        }


        protected static HttpClient client;

        protected const string plotUrl = "https://hbnpnzvanc.execute-api.us-east-1.amazonaws.com/Beta/graph?uid=";
        protected const string associationsUrl = "https://hbnpnzvanc.execute-api.us-east-1.amazonaws.com/Beta/sensor-association";
        protected const string associationsGetUrl = associationsUrl + "?uid=room1";


        protected class PlotImage
        {
            public DateTime time;
            public UIImage image;
        }
        //TODO automatic updating
        protected Dictionary<string, PlotImage> imageCache = new Dictionary<string, PlotImage>();

        static AWSRestStore()
        {
            client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(60.0);
        }
        public AWSRestStore()
        {
            ImageUpdateDelta = TimeSpan.FromSeconds(30.0);
        }

        // get the png data from the server.
        protected UIImage FetchImage(string plotId)
        {
            try
            {
                string uri = plotUrl + plotId;
                Debug.Print("FetchImageAsync uri " + uri);
                var response = client.GetAsync(uri).Result;
                Debug.Print("response.StatusCode: " + response.StatusCode);
                if (response.IsSuccessStatusCode)
                {
                    Debug.Print(response.ToString());
                    Stream imageStream = response.Content.ReadAsStreamAsync().Result;
                    Byte[] buff = new Byte[imageStream.Length];
                    Debug.Print("imageStream.Length " + imageStream.Length + " buff.length " + buff.Length);
                    int numRead = imageStream.ReadAsync(buff, 0, (int)imageStream.Length).Result;
                    Debug.Print("numRead: " + numRead);
                    CGDataProvider dataProvider = new CGDataProvider(buff);
                    CGImage cgImage = CGImage.FromPNG(dataProvider, null, false, CGColorRenderingIntent.Default);
                    Debug.Print("cgImage height: " + cgImage.Height + " cgImage width: " + cgImage.Width);
                    UIImage uiImage = UIImage.FromImage(cgImage);
                    return uiImage;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.Print("Exception in FetchImageAsync");
                Debug.Print(e.Message);
                return null;
            }
        }


        // check the cache, and if approriate, get data from the server for a plot image.
        public UIImage GetPlot(string plotId)
        {
            Debug.PrintWT("GetPlot imageCache size " + imageCache.Count);
            PlotImage pImage = null;
            if (imageCache.TryGetValue(plotId, out pImage))
            {// check if its too old
                TimeSpan timeSpan = DateTime.Now.Subtract(pImage.time);
                Debug.Print("last update to image was " + timeSpan.Seconds + " seconds ago.");
                if (timeSpan.CompareTo(imageUpdateDelta) >= 0) // passed our delta
                {//TODO network exceptions?
                    Debug.Print("Updating an old image for " + plotId);
                    UIImage image = FetchImage(plotId);
                    pImage.time = DateTime.Now;
                    pImage.image = image;
                    return image;
                }
                else
                {//return cached
                    Debug.Print("Getting cached image for " + plotId);
                    return pImage.image;
                }
            }
            else
            {
                Debug.Print("Getting a new image for " + plotId);
                UIImage image = FetchImage(plotId);
                pImage = new PlotImage();
                pImage.time = DateTime.Now;
                pImage.image = image;
                imageCache.Add(plotId, pImage);
                return image;
            }
        }

        // store associates in the aws db.
        public void SetAssociations(SensorAssociations associations)
        {
            Debug.Print("SetAssociations()");
            if (associations == null)
            {
                throw new ArgumentNullException("SetAssociations(SensorAssociations associations) associations can't be null");
            }
            
            try
            {
                string associationsJson = JsonConvert.SerializeObject(associations);
                AssociationsWrapper wrapper = new AssociationsWrapper
                {
                    data = associationsJson
                };
                string json = JsonConvert.SerializeObject(wrapper);
                Debug.Print("Storing associations " + json);

                HttpRequestMessage request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(associationsUrl),
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                //request.Headers.Add("Content-Type", "application/json");

                var response = client.SendAsync(request).Result;
                Debug.Print("response.StatusCode: " + response.StatusCode);
            }
            catch (Exception e)
            {
                Debug.Print("SetAssociations Exception " + e.Message);
            }
        }

        // read associations from the aws db.
        public SensorAssociations GetAssociations()
        {
            Debug.Print("GetAssociations()");
            try
            {
                var response = client.GetAsync(associationsGetUrl).Result;
                Debug.Print("response.StatusCode: " + response.StatusCode);
                if (response.IsSuccessStatusCode)
                {
                    Debug.Print(response.ToString());
                    string json  = response.Content.ReadAsStringAsync().Result;
                    AssociationsWrapper wrapper = JsonConvert.DeserializeObject<AssociationsWrapper>(json);
                    if (wrapper == null || wrapper.data == null)
                    {
                        Debug.Print("wrapper == null || wrapper.data == null");
                        return new SensorAssociations();
                    }
                    else
                    {
                        SensorAssociations associations = JsonConvert.DeserializeObject<SensorAssociations>(wrapper.data);
                        if (associations == null)
                        {
                            Debug.Print("associations == null");
                            return new SensorAssociations();
                        }
                        else
                        {
                            return associations;
                        }

                    }
                }
                else
                {
                    return new SensorAssociations();
                }
            }
            catch (Exception e)
            {
                Debug.Print("Getting associations Exception " + e.Message);
                return new SensorAssociations();
            }
        }
    }
}