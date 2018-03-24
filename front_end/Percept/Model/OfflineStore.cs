using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Newtonsoft.Json;
using Percept.Utility;

namespace Percept.Model
{
    //use NSUserDefaults to mock the db, and store mapping data offline.
    public class OfflineStore : IPersistentDataManager
    {
        //example
        //public static void RegisterDefaults()
        //{
        //    DragOnInfinitePlanes = true;
        //    ScaleWithPinchGesture = true;
        //}

        //public static bool ScaleWithPinchGesture
        //{
        //    get { return NSUserDefaults.StandardUserDefaults.BoolForKey("ScaleWithPinchGesture"); }
        //    set { NSUserDefaults.StandardUserDefaults.SetBool(value, "ScaleWithPinchGesture"); }
        //}

        //public static bool DragOnInfinitePlanes
        //{
        //    get { return NSUserDefaults.StandardUserDefaults.BoolForKey("DragOnInfinitePlanes"); }
        //    set { NSUserDefaults.StandardUserDefaults.SetBool(value, "DragOnInfinitePlanes"); }
        //}



        public SensorAssociations GetAssociations()
        {
            try
            {
                string json = NSUserDefaults.StandardUserDefaults.StringForKey("associations");
                Debug.Print("Getting associations" + json);
                if (json == null)
                {
                    Debug.Print("OfflineStore Could not find previous associations");
                    return new SensorAssociations();
                }
                else
                {
                    AssociationsWrapper wrapper = JsonConvert.DeserializeObject<AssociationsWrapper>(json);
                    if (wrapper == null || wrapper.data == null)
                    {
                        Debug.Print("wrapper == null || wrapper.data == null");
                        return new SensorAssociations();
                    }
                    else
                    {
                        SensorAssociations associations = JsonConvert.DeserializeObject<SensorAssociations>(wrapper.data);
                        if(associations == null)
                        {
                            Debug.Print("associations == null");
                            return new SensorAssociations();
                        }else
                        {
                            return associations;
                        }

                    }
                }
            }catch(Exception e)
            {
                Debug.Print("Getting associations Exception" + e.Message);
                return new SensorAssociations();
            }
            
        }

        public UIImage GetPlot(string plotId)
        {
            throw new NotImplementedException();
        }

        public void SetAssociations(SensorAssociations associations)
        {
            if(associations == null)
            {
                throw new ArgumentNullException("SetAssociations(SensorAssociations associations) associations can't be null");
            }
            string associationsJson = JsonConvert.SerializeObject(associations);
            AssociationsWrapper wrapper = new AssociationsWrapper
            {
                data = associationsJson
            };
            string json = JsonConvert.SerializeObject(wrapper);
            Debug.Print("Storing associations " + json);
            NSUserDefaults.StandardUserDefaults.SetString(json, "associations");
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }


    }
}