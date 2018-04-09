using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARKit;
using Foundation;
using Percept.ObjectExtensions;
using UIKit;

namespace Percept.Model
{
    public class HitDistanceAverager
    {

        //how many samples we are working with,
        protected nfloat averageSize;
        //to avoid casting when making queues
        protected int averageSizeInt;
        //cache
        protected int midIndex;
        protected FixedSizeQueue<HitObject> objectHitDistances;



        public HitDistanceAverager(int numSamples)
        {
            if (numSamples < 2)
            {
                throw new ArgumentOutOfRangeException("numSamples should be >= 2");
            }
            else if (numSamples % 2 == 0)
            {
                throw new ArgumentException("numSamples should odd");
            }
            else
            {
                averageSize = numSamples;
                averageSizeInt = numSamples;
                midIndex = numSamples / 2;
                objectHitDistances = new FixedSizeQueue<HitObject>(averageSizeInt);
            }

        }

        public bool GetLatestSample(out HitObject sample)
        {
            if (objectHitDistances.Length < 1)
            {
                sample = null;
                return false;
            }
            sample = objectHitDistances.PeekLatest();
            return true;
        }

        public void AddSample(ARHitTestResult hitResult)
        {
            if (hitResult == null)
            {
                return;
            }
            HitObject sampleHit = new HitObject
            {
                Transform = hitResult.WorldTransform,
                Distance = hitResult.Distance
            };
            objectHitDistances.Enqueue(sampleHit);
        }

        public bool GetMedian(out HitObject median)
        {
            if (objectHitDistances.IsFull())
            {
                //we can implement selection based median if our samples get large
                var list = objectHitDistances.ToList();
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

        //return true if the median is ready
        public bool AddSampleGetMedian(ARHitTestResult hitResult, out HitObject median)
        {
            if (hitResult == null)
            {
                median = null;
                return false;
            }
            HitObject sampleHit = new HitObject
            {
                Transform = hitResult.WorldTransform,
                Distance = hitResult.Distance
            };

            objectHitDistances.Enqueue(sampleHit);
            if (objectHitDistances.IsFull())
            {
                //we can implement selection based median if our samples get large
                var list = objectHitDistances.ToList();
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
    }
}