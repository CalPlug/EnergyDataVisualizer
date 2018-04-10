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
using CoreGraphics;
using SpriteKit;

namespace Percept
{
    public abstract partial class PerceptViewController : UIViewController, IARViewControllerDelegate , IARSCNViewDelegate, IARSessionDelegate, IVirtualObjectManagerDelegate
    {
        protected ARSCNView snView;
        protected SCNScene scene;

        //useful ephemeral stuff
        protected ARFrame currFrame;
        protected CVPixelBuffer currPixelbuff;

        //use for reading and writing scene related data.
        protected DispatchQueue serialSceneQ { get; set; }

        protected static CGPoint MIDPOINT = new CGPoint(0.5, 0.5);

        //how much distance the plot should be away from the camera when it's first made.
        protected static SCNVector3 SENSOR_DISPLAY_CAMERA_OFFSET = new SCNVector3(0f,0f,-0.5f);//.5m in front of camera
        protected static SCNMatrix4 SENSOR_DISPLAY_CAMERA_OFFSET_TRANSFORM = SCNMatrix4.CreateTranslation(SENSOR_DISPLAY_CAMERA_OFFSET);
        protected static SCNQuaternion Y_FLIP_QUAT = SCNQuaternion.FromAxisAngle(new SCNVector3(0f, 1f, 0f), (Single)(Math.PI));

        //true if we wish to remove rotation about the camera relative z axis when adding planes.
        protected bool zOrientCorrection = true;

        // the aws data base for plots and associations
        protected IPersistentDataManager store = new AWSRestStore();
        // a queue for image update tasks
        protected DispatchQueue serialStoreQueue;
        // poll delta
        protected static TimeSpan POLL_DELTA = TimeSpan.FromSeconds(120.0);
        protected Action pollStoreImages;

        //map sensor ids to  to sensor displays, the inverse is also mapped with the name property of scnnodes.
        protected Dictionary<string, SensorDisplay> idToSensorDisplay = new Dictionary<string, SensorDisplay>();

        // the object uid that we can currently map, because we have it picked.
        protected string selectedDisplayId = null;
        // the last time the object was tapped (so we could implement double tap)'
        protected DateTime? lastSelectedTime = null;
        // time in between taps to consider a double tap
        protected static TimeSpan doubleTapDelta = TimeSpan.FromMilliseconds(500);
        // to overlay the zoomin plot
        protected SKScene skScene = null;
        protected SensorDisplayZoomIn zoomIn;

        protected HitDistanceAverager latestHits = new HitDistanceAverager(15);

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

            skScene = new SKScene();
            zoomIn = new SensorDisplayZoomIn(() => HandleZoomInTouch());
            skScene.AddChild(zoomIn);

            serialStoreQueue = new DispatchQueue(label: "com.xamarin.Percept.serialStoreQueue");
            pollStoreImages = (() =>UpdateSensorDisplays());

            serialSceneQ = new DispatchQueue(label: "com.xamarin.Percept.serialSceneQ");
            UserFeedback = new TextManager(this);

            virtualObjectManager = new VirtualObjectManager(serialSceneQ, this, scene);

 

            snView.Session.Delegate = this;
            //The following requires camera permissions in Info.plist, otherwise we crash (try catch doesn't work)
            ResetTracking();
            serialStoreQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, POLL_DELTA), pollStoreImages);
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

        protected void HandleZoomInTouch()
        {
            Debug.Print("HandleZoomInTouch");
            if (lastSelectedTime != null)
            {
                //it's already been selected, check for a double tap to zoom in
                TimeSpan delta = DateTime.Now.Subtract(lastSelectedTime.Value);
                if (delta.CompareTo(doubleTapDelta) <= 0)
                {
                    lastSelectedTime = null;
                    serialSceneQ.DispatchAsync(() => ZoomOutPlot());
                }
                else
                {
                    lastSelectedTime = DateTime.Now;
                }
            }
            else
            {
                lastSelectedTime = DateTime.Now;
            }
        }

        protected void UpdateSensorDisplays()
        {
            Debug.Print("UpdateSensorDisplays()");
            foreach (var kv in idToSensorDisplay)
            {
                SensorDisplay display = kv.Value;
                display.UpdateContents(store.GetPlot(kv.Key));
            }
            serialStoreQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, POLL_DELTA), pollStoreImages);
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

        // zooms in on the selectedDisplayId plot
        protected void ZoomInPlot()
        {
            Debug.Print("ZoomInPlot");
            UIImage plot = store.GetPlot(selectedDisplayId);
            if (plot == null)
            {
                return;
            }
            InvokeOnMainThread(() =>
            {
                zoomIn.SetContents(plot);
                snView.OverlayScene = skScene;
            });
        }

        // zooms in on the selectedDisplayId plot
        protected void ZoomOutPlot()
        {
            Debug.Print("ZoomOutPlot");
            InvokeOnMainThread(() =>
            {
                snView.OverlayScene = null;
            });
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

            SCNMatrix4 transform = cameraOffsetTransfom.HasValue ?
                cameraNode.ConvertTransformToNode(cameraOffsetTransfom.Value, display) :
                cameraNode.ConvertTransformToNode(SENSOR_DISPLAY_CAMERA_OFFSET_TRANSFORM, display);
            //Debug.Print("transform:\n" + transform);
            display.Transform = transform;

            SCNVector3 camPos = cameraNode.Position;
            //to zero out the y axis so the plot doesn't pitch forward or backward and faces the user (backwards / culled).
            display.Look(new SCNVector3(camPos.X, display.Position.Y, camPos.Z));

            display.LocalRotate(Y_FLIP_QUAT);

            // remove twisting on the z rot axis due to tablet rotation passed on thru camera transform.
            if (zOrientCorrection)
            {
                SCNQuaternion displayQ = display.Orientation;
                SCNQuaternion twist = SCNQuaternionExtensions.TwistDecompositon(SCNVector3Extensions.UNIT_Z_VEC, displayQ);

                SCNQuaternion twistConj = SCNQuaternion.Conjugate(twist);

                SCNQuaternion finalOrientation = SCNQuaternion.Multiply(displayQ, twistConj);
                display.Orientation = finalOrientation;
            }
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

            //do a hitttest from center of screen.
            ARHitTestResult centerHit = FeatureHitTestFromPoint(MIDPOINT);
            if (centerHit != null)
            {
                latestHits.AddSample(centerHit);
            }
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

        // GC.KeepAlive(currFrame); is used to keep other threads from deleting this frame until it is not needed.
        // on the other hand if we don't dispose of it soon enough, our application doesn't render.
        protected ARHitTestResult FeatureHitTestFromPoint(CGPoint point)
        {
            if (currFrame == null || point == null)
            {
                return null;
            }
            ARHitTestResult[] hitTestResults = null;
            try
            {

                hitTestResults = currFrame.HitTest(point,
                ARHitTestResultType.FeaturePoint | ARHitTestResultType.ExistingPlaneUsingGeometry
                | ARHitTestResultType.EstimatedHorizontalPlane);
                if (hitTestResults != null && hitTestResults.Length > 0)
                {
                    return hitTestResults[0];
                }
                GC.KeepAlive(currFrame);
                return null;
            }
            catch (Exception e)
            {
                Debug.Print("FeatureHitTestFromPoint " + e.Message);
                return null;
            }
        }

        public abstract void CouldNotPlace(VirtualObject virtualObject);

        public abstract void ObjectTapped(VirtualObject virtualObject);

        public abstract void NothingTapped();

        public abstract void TransformDidChangeFor(VirtualObject virtualObject);

        public abstract void TranslationFinishedFor(VirtualObject virtualObject);
    }
}

