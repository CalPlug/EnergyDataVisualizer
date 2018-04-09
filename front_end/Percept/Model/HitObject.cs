using System;
using System.Collections.Generic;
using System.Text;

namespace Percept.Model
{
    // strips ARHitTestResult from unnecessary data
    public class HitObject
    {
        public nfloat Distance { get; set; }
        public OpenTK.NMatrix4 Transform { get; set; }

        public static int CompareHitObjects(HitObject a, HitObject b)
        {
            return a.Distance.CompareTo(b.Distance);
        }

        public static Comparison<HitObject> Comparator = new Comparison<HitObject>(CompareHitObjects);
    }
}
