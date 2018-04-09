using System;
using System.Collections.Generic;
using System.Linq;
using ARKit;
using Percept.ObjectExtensions;

namespace Percept.Model
{
    // used to throw away crazy hit test results by keeping a buffer of sampled hit points.
    public class ObjectHitDistanceAverager
    {
        //how many samples we are working with,
        protected nfloat averageSize;
        //to avoid casting when making queues
        protected int averageSizeInt;
        //cache
        protected int midIndex;
        protected Dictionary<string, FixedSizeQueue<HitObject>> objectHitDistances;

        public ObjectHitDistanceAverager(int numSamples)
        {
            if(numSamples < 2)
            {
                throw new ArgumentOutOfRangeException("numSamples should be >= 2");
            }else if(numSamples % 2 == 0)
            {
                throw new ArgumentException("numSamples should odd");
            }
            else
            {
                averageSize = numSamples;
                averageSizeInt = numSamples;
                midIndex = numSamples / 2;
                objectHitDistances = new Dictionary<string, FixedSizeQueue<HitObject>>();
            }

        }

        //return true if the median is ready
        public bool AddSampleGetMedian(string objectName, ARHitTestResult hitResult, out HitObject median)
        {
            if(hitResult == null)
            {
                median = null;
                return false;
            }
            HitObject sampleHit = new HitObject
            {
                Transform = hitResult.WorldTransform,
                Distance = hitResult.Distance
            };

            if (objectHitDistances.TryGetValue(objectName, out FixedSizeQueue<HitObject> samples))
            {
                samples.Enqueue(sampleHit);
                if (samples.IsFull())
                {
                    //we can implement selection based median if our samples get large
                    var list = samples.ToList();
                    list.Sort(HitObject.Comparator);
                    median = list[midIndex];
                    return true;
                }else
                {
                    median = null;
                    return false;
                }
            }
            else //it's our first sample
            {
                FixedSizeQueue<HitObject> q = new FixedSizeQueue<HitObject>(averageSizeInt);
                q.Enqueue(sampleHit);
                objectHitDistances.Add(objectName, q);
                median = null;
                return false;
            }
        }

        //return true if the median is ready
        public bool AddSampleGetMedian(string objectName, HitObject sampleHit, out HitObject median)
        {
            if (sampleHit == null)
            {
                median = null;
                return false;
            }

            if (objectHitDistances.TryGetValue(objectName, out FixedSizeQueue<HitObject> samples))
            {
                samples.Enqueue(sampleHit);
                if (samples.IsFull())
                {
                    //we can implement selection based median if our samples get large
                    var list = samples.ToList();
                    list.Sort(HitObject.Comparator);
                    median = list[midIndex];
                    return true;
                }
                else
                {
                    median = null;
                    return false;
                }
            }
            else //it's our first sample
            {
                FixedSizeQueue<HitObject> q = new FixedSizeQueue<HitObject>(averageSizeInt);
                q.Enqueue(sampleHit);
                objectHitDistances.Add(objectName, q);
                median = null;
                return false;
            }
        }
    }
}