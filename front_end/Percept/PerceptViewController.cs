using ARKit;
using System;
using UIKit;
using CoreFoundation;
using Percept.Utility;
using Percept.SCNNodes;
using SceneKit;
using Foundation;
using System.Collections.Generic;
using Percept.Model;
using Percept.ObjectExtensions;
using CoreVideo;
using CoreLocation;
using Percept.UIElements;

namespace Percept
{
    public abstract partial class PerceptViewController : UIViewController, IARViewControllerDelegate , IARSCNViewDelegate, IARSessionDelegate
	{
        protected ARSCNView snView;
        protected SCNScene scene;

        //useful ephemeral stuff
        protected ARFrame currFrame;
        protected CVPixelBuffer currPixelbuff;

        //use for reading and writing scene related data.
        protected DispatchQueue serialSceneQ { get; set; }

        //how much distance the plot should be away from the camera when it's first made.
        protected static SCNVector3 SENSOR_DISPLAY_CAMERA_OFFSET = new SCNVector3(0f,0f,-0.5f);//.5m in front of camera
        protected static SCNMatrix4 SENSOR_DISPLAY_CAMERA_OFFSET_TRANSFORM = SCNMatrix4.CreateTranslation(SENSOR_DISPLAY_CAMERA_OFFSET);
        protected static SCNQuaternion Y_FLIP_QUAT = SCNQuaternion.FromAxisAngle(new SCNVector3(0f, 1f, 0f), (Single)(Math.PI));

        //true if we wish to remove rotation about the camera relative z axis when adding planes.
        protected bool zOrientCorrection = true;

        // the aws data base for plots and associations
        protected IPersistentDataManager store = new AWSRestStore();

        //map sensor ids to  to sensor displays, the inverse is also mapped with the name property of scnnodes.
        protected Dictionary<string, SensorDisplay> idToSensorDisplay = new Dictionary<string, SensorDisplay>();
        //map sensor ids to  to sensor models
        //protected Dictionary<string, SerializableSensorDisplay> Sensors = new Dictionary<string, SerializableSensorDisplay>();


        //3d objects
        protected VirtualObjectManager virtualObjectManager;

        protected TextManager UserFeedback;



        public PerceptViewController(IntPtr handle) : base(handle)
        {
        }


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Debug.Print("PerceptViewController ViewDidLoad()");

            //storyboard check
            if(View is ARSCNView)
            {
                Debug.Print("PerceptViewController was made with an ARSCNView");
                snView = (ARSCNView)View;
            }
            else
            {
                Debug.Print("PerceptViewController was NOT made with an ARSCNView");
                View = snView = new ARSCNView();
            }

            //To prevent null reference exception when going back to menu
            
            snView.Delegate = this;
            scene = new SCNScene();
            snView.Scene = scene;
 

            serialSceneQ = new DispatchQueue(label: "com.xamarin.Percept.serialSceneQ");
            UserFeedback = new TextManager(this);

            virtualObjectManager = new VirtualObjectManager(serialSceneQ, this, scene);

 

            snView.Session.Delegate = this;
            //The following requires camera permissions in Info.plist, otherwise we crash (try catch doesn't work)
            ResetTracking();
        }

        [Action("RestartExperience:")]
        public void RestartExperience(NSObject sender)
        {
            //if (!RestartExperienceButton.Enabled || IsLoadingObject)
            //{
            //    return;
            //}

            //RestartExperienceButton.Enabled = false;

            UserFeedback.CancelAllScheduledMessages();
            UserFeedback.DismissPresentedAlert();
            UserFeedback.ShowMessage("STARTING A NEW SESSION");

            //virtualObjectManager.RemoveAllVirtualObjects();

            ResetTracking();

            //RestartExperienceButton.SetImage(UIImage.FromBundle("restart"), UIControlState.Normal);
        }

        protected ARConfiguration GetStandardConfig()
        {
            // Create a session configuration

            var configuration = new ARWorldTrackingConfiguration
            {
                PlaneDetection = ARPlaneDetection.Vertical | ARPlaneDetection.Horizontal,
                AutoFocusEnabled = true,
                LightEstimationEnabled = false, // might not need this to just display a plot.
                // +y = +g.  if we do gravity and heading, then +x is east and +z is south.
                WorldAlignment = GetAlignment()
            };

            Debug.Print("WorldAlignment is " + configuration.WorldAlignment.ToString());
            return configuration;
        }

        protected void ResetTracking()
        {
            snView.Session.Run(GetStandardConfig(), ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors);
            UserFeedback.ScheduleMessage("SESSION HAS RESET", 7.5, MessageType.PlaneEstimation);
        }

        protected ARWorldAlignment GetAlignment()
        {
            switch (CLLocationManager.Status)
            {
                case CLAuthorizationStatus.Authorized:
                case CLAuthorizationStatus.AuthorizedWhenInUse:
                    return ARWorldAlignment.GravityAndHeading;
                default:
                    return ARWorldAlignment.Gravity;
            }
        }
        protected virtual void PlaceInFrontOfCamera(SCNNode node, SCNMatrix4? cameraOffsetTransfom = null)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node can't be null");
            }
            SCNNode cameraNode = snView.PointOfView;

            SCNMatrix4 transform = cameraOffsetTransfom.HasValue ?
                cameraNode.ConvertTransformToNode(cameraOffsetTransfom.Value, node) :
                cameraNode.ConvertTransformToNode(SENSOR_DISPLAY_CAMERA_OFFSET_TRANSFORM, node);
            node.Transform = transform;
        }

        //add a new display infront of camera
        protected virtual void SetSensorDisplayPositionFromCamera(SensorDisplay display, SCNMatrix4? cameraOffsetTransfom = null)
        {
            if(display == null)
            {
                throw new ArgumentNullException("SensorDisplay can't be null");
            }
            ARCamera camera = currFrame.Camera;
            SCNNode cameraNode = snView.PointOfView;

            //Debug.Print("cameraNode Transform:\n" + cameraNode.Transform);
            //Debug.Print("cameraNode Position:\n" + cameraNode.Position);
            //Debug.Print("cameraNode Rotation:\n" + cameraNode.Rotation);
            //Debug.Print("cameraNode Orientation:\n" + cameraNode.Orientation);



            SCNMatrix4 transform = cameraOffsetTransfom.HasValue ?
                cameraNode.ConvertTransformToNode(cameraOffsetTransfom.Value, display) :
                cameraNode.ConvertTransformToNode(SENSOR_DISPLAY_CAMERA_OFFSET_TRANSFORM, display);
            //Debug.Print("transform:\n" + transform);
            display.Transform = transform;

            SCNVector3 camPos = cameraNode.Position;
            //to zero out the y axis so the plot doesn't pitch forward or backward and faces the user (backwards / culled).
            display.Look(new SCNVector3(camPos.X, display.Position.Y, camPos.Z));

            //Debug.Print("display transform before quat:\n" + display.Transform);
            //Debug.Print("display euler angles before quat:\n" + display.EulerAngles);
            //Debug.Print("display orientation before quat:\n" + display.Orientation);

            display.LocalRotate(Y_FLIP_QUAT);
            //Debug.Print("display transform after quat:\n" + display.Transform);
            //Debug.Print("display euler angles after quat:\n" + display.EulerAngles);
            //Debug.Print("\tdisplay orientation after quat:\n" + display.Orientation);

            if (zOrientCorrection)
            {
                //remove camera relative roll
                // (-0.0007383518, 0.1308612, -0.005658311)  forward, no roll
                // (-0.08546476,   0.1056983, -0.6820469)   forward, roll
                // (-3.13949,     -0.1361102,  3.126097)    backward, no roll
                // ( 3.04493,     -0.1060119, -2.399804)    backward, roll

                SCNQuaternion displayQ = display.Orientation;
                SCNQuaternion twist = SCNQuaternionExtensions.TwistDecompositon(SCNVector3Extensions.UNIT_Z_VEC, displayQ);
                //Debug.Print("twist\n" + twist);

                SCNQuaternion twistConj = SCNQuaternion.Conjugate(twist);
                //Debug.Print("twistConj\n" + twistConj);

                SCNQuaternion finalOrientation = SCNQuaternion.Multiply(displayQ, twistConj);
                //Debug.Print("finalOrientation\n" + finalOrientation);

                display.Orientation = finalOrientation;
            }

            //Sensors.Add(id, new SerializableSensorDisplay(id, display.WorldTransform)); //what's the point if transforms are going to change.
        }

        public override bool ShouldAutorotate ()
		{
			return true;
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations ()
		{
			return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ? UIInterfaceOrientationMask.AllButUpsideDown : UIInterfaceOrientationMask.All;
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            virtualObjectManager.ReactToTouchesBegan(touches, evt, snView);
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            virtualObjectManager.ReactToTouchesMoved(touches, evt);
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            virtualObjectManager.ReactToTouchesEnded(touches, evt);
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            virtualObjectManager.ReactToTouchesCancelled(touches, evt);
        }

        [Export("session:didUpdateFrame:")]
        public virtual void DidUpdateFrame(ARSession session, ARFrame frame)
        {
            if(currFrame != null)
            {
                currFrame.Dispose();
            }
            currFrame = frame;

            if(currPixelbuff != null)
            {
                currPixelbuff.Dispose();

            }
            currPixelbuff = currFrame.CapturedImage;

        }

        public ARFrame GetCurrentFrame()
        {
            return currFrame;
        }

        public CVPixelBuffer GetCurrentPixelBuffer()
        {
            return currPixelbuff;
        }

        public SCNView GetView()
        {
            return snView;
        }

        public abstract UILabel GetMessageLabel();

    }
}

