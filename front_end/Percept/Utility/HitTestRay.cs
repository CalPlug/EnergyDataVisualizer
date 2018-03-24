// Taken from https://developer.xamarin.com/samples/monotouch/ios11/ARKitPlacingObjects/
// A Xamarin port of the Placing Objects Sample from Apple
// MIT License
using Foundation;
using SceneKit;

namespace Percept.Model
{
    public class HitTestRay : NSObject
	{
		public SCNVector3 Origin { get; set; }

		public SCNVector3 Direction { get; set; }

		public HitTestRay()
		{
		}

		public HitTestRay(SCNVector3 origin, SCNVector3 direction)
		{
			// Initialize
			Origin = origin;
			Direction = direction;
		}

        //TODO: by modifying the sample code to make this a member func of hittestray I may have messed up some copying mechanics 
        //(things that should be copy are now a reference) - could be a cause of some bugs
        public  SCNVector3? RayIntersectionWithHorizontalPlane(float planeY)
        {
            // Normalize direction
            SCNVector3 direction = SCNVector3.Normalize(Direction);

            // Special case handling: Check if the ray is horizontal as well.
            if (direction.Y == 0)
            {
                if (Origin.Y == planeY)
                {
                    // The ray is horizontal and on the plane, thus all points on the ray intersect with the plane.
                    // Therefore we simply return the ray origin.
                    return Origin;
                }
                else
                {
                    // The ray is parallel to the plane and never intersects.
                    return null;
                }
            }

            // The distance from the ray's origin to the intersection point on the plane is:
            //   (pointOnPlane - rayOrigin) dot planeNormal
            //  --------------------------------------------
            //          direction dot planeNormal

            // Since we know that horizontal planes have normal (0, 1, 0), we can simplify this to:
            var dist = (planeY - Origin.Y) / direction.Y;

            // Do not return intersections behind the ray's origin.
            if (dist < 0)
            {
                return null;
            }

            // Return the intersection point.
            return SCNVector3.Add(Origin, direction * dist);
        }

    }
}
