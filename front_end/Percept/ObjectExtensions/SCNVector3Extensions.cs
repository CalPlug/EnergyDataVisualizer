// Partially Taken from https://developer.xamarin.com/samples/monotouch/ios11/ARKitPlacingObjects/
// A Xamarin port of the Placing Objects Sample from Apple
// MIT License
using System;
using CoreGraphics;

namespace Percept.ObjectExtensions
{
    public static class SCNVector3Extensions {

        public static SceneKit.SCNVector3 UNIT_Z_VEC = new SceneKit.SCNVector3(0f, 0f, 1f);

        public static CGPoint ToCGPoint(this SceneKit.SCNVector3 self)
        {
            return new CGPoint(self.X, self.Y);
        }

        public static OpenTK.NMatrix4 ToTranslationMatrix(this SceneKit.SCNVector3 self)
        {
            OpenTK.NMatrix4 matrix = OpenTK.NMatrix4.Identity.Copy();
            matrix.M41 = self.X;
            matrix.M42 = self.Y;
            matrix.M43 = self.Z;
            return matrix;
        }

        // (a.x - b.x)^2 + ...
        public static Single SquaredDifferenceSum(ref SceneKit.SCNVector3 a, ref SceneKit.SCNVector3 b)
        {
            Double sum = 0;
            SceneKit.SCNVector3 diff = SceneKit.SCNVector3.Subtract(a, b);
            sum = sum + Math.Pow(diff.X, 2);
            sum = sum + Math.Pow(diff.Y, 2);
            sum = sum + Math.Pow(diff.Z, 2);
            return (Single)sum;
        }

        public static Single Distance(SceneKit.SCNVector3 a, SceneKit.SCNVector3 b)
        {
            Single dx = a.X - b.X;
            Single dy = a.Y - b.Y;
            Single dz = a.Z - b.Z;
            return (Single) Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        //Does not work - a reminder
        //public static void Add(this ref SceneKit.SCNVector3 self, SceneKit.SCNVector3 other)
        //{
        //    self.X = self.X + other.X;
        //    self.Y = self.Y + other.Y;
        //    self.Z = self.Z + other.Z;
        //}

        public static SceneKit.SCNVector3 PositionFromTransform(OpenTK.NMatrix4 transform)
        {
            var pFromComponents = new SceneKit.SCNVector3(transform.M14, transform.M24, transform.M34);
            return pFromComponents;
        }
    }
}
