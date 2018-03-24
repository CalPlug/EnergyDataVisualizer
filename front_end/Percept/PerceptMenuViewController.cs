using CoreLocation;
using System;

using UIKit;

namespace Percept
{
    public partial class PerceptMenuViewController : UIViewController
    {
        public PerceptMenuViewController(IntPtr handle) : base(handle)
        {
        }

    public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        // set the location popup here, so that we can use gps for heading.
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            switch (CLLocationManager.Status)
            {
                case CLAuthorizationStatus.Authorized:
                case CLAuthorizationStatus.AuthorizedWhenInUse:
                    return ;
                default:
                    CLLocationManager location = new CLLocationManager();
                    location.RequestAlwaysAuthorization();
                    break;
            }
            // Perform any additional setup after loading the view, typically from a nib.
        }
    }
}