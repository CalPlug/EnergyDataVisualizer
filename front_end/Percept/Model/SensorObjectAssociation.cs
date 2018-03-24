using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Percept.Model
{
    public class SensorAssociations
    {
        public Dictionary<string, Tuple<string, float>> SensorIdToObject { get; set; }
        public Dictionary<string, Tuple<string, float>> ObjectToSensorId { get; set; }

        public SensorAssociations()
        {
            SensorIdToObject = new Dictionary<string, Tuple<string, float>>();
            ObjectToSensorId = new Dictionary<string, Tuple<string, float>>();
        }

        public void AddAssociation(string sensorId, string objectName, float confidence)
        {
            //enforce 1-1 mapping by deleting any previous stuff
            RemoveAssociationSensorId(sensorId);
            RemoveAssociationObject(objectName);

            SensorIdToObject.Add(sensorId,new Tuple<string, float>(objectName, confidence));
            ObjectToSensorId.Add(objectName , new Tuple<string, float>(sensorId, confidence));
        }

        public void RemoveAssociationObject(string objectName)
        {
            ObjectToSensorId.TryGetValue(objectName, out Tuple<string, float> sensor);
            if(sensor != null)
            {
                SensorIdToObject.Remove(sensor.Item1);
                ObjectToSensorId.Remove(objectName);
            }
        }

        public void RemoveAssociationSensorId(string sensorId)
        {
            SensorIdToObject.TryGetValue(sensorId, out Tuple<string, float> objectName);
            if (objectName != null)
            {
                ObjectToSensorId.Remove(objectName.Item1);
                SensorIdToObject.Remove(sensorId);
            }
        }
    }
}