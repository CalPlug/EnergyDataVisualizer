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
using Newtonsoft.Json;
using Percept.ObjectExtensions;
using Vision;
using CoreGraphics;
using CoreVideo;
using CoreImage;
using ImageIO;
using System.Threading.Tasks;
using CoreML;
using Percept.UIElements;

namespace Percept
{
    public partial class PerceptQRViewController : PerceptViewController, IVirtualObjectManagerDelegate, IARSCNViewDelegate
    {

        public PerceptQRViewController(IntPtr handle) : base(handle) { }

        // maybe we can combine this one with ar tracking quality or hittest point density
        protected const Single QRCODE_DISTANCE_THRESH = 0.2f;
        // for flickering
        protected const Single ANCHOR_SQUARED_DIFFERENCE_THROWAWAY_THRESH = 0.01f;
        // to stop unnecessary updates in sensor displays from qr code locations
        protected const Single CAMERA_DIFFERENCE_THROWAWAY_THRESH = 0.005f;
        // the kind of codes that we are detecting
        protected VNBarcodeSymbology[] symbologies = { VNBarcodeSymbology.QR };
        // task queue for image processing.
        protected DispatchQueue serialImageQueue;
        // handler for a qr image reading / recognition
        protected FixVNDetectBarcodesRequest qrRequest;
        // the aws data base for plots and associations
        protected IPersistentDataManager store = new AWSRestStore();
        // our object mappings
        protected SensorAssociations associations;

        bool loadingImage = false;
        // to prevent unnecessary image queue dispatch
        bool analyzingFrame = false;

        // all the stuff for mobilenet image recognition / object classification
        protected MLModel imageMLModel;
        protected VNCoreMLModel imageVNModel;
        protected VNCoreMLRequest mlRequest;
        // how many of the top CLASSIFICATIONS_SIZE classifications that we consider, when showing our associations selection
        // picker and how many we look at when we try to detect objects and show sensor displays to the user.
        protected static int CLASSIFICATIONS_SIZE = 3;
        // how much worse confidence can be when detecting the same object.
        protected static float CONFIDENCE_DELTA = 0.05f;
        // TODO we could possibly persist distance to help place at the right spot
        protected HitDistanceAverager hitDistanceAverager = new HitDistanceAverager(5);
        // can have all nulls or 1 value, 4 nulls, etc.. must set used manually
        protected FixedArray<VNClassificationObservation> lastClassifications =
            new FixedArray<VNClassificationObservation>(CLASSIFICATIONS_SIZE);
        // used to control picker behavior and data.
        protected ClassificationPickerModel pickerModel;

        // the object uid that we can currently map, because we have it picked.
        protected string selectedDisplayId = null;

        //              sensorid   static hit point
        protected Dictionary<string, Point> displayPoints;
        //              sensorid   static line
        protected Dictionary<string, SCNNode> displayLines;
        // color of the lines that go between points and sensor displays
        protected static UIColor DISPLAY_COLOR = UIColor.FromRGBA(190, 255, 255, 225);
        protected static SCNMaterial[] DISPLAY_MAT = new SCNMaterial[] { new SCNMaterial() };
        // thickness of pointing line
        protected static Single LINE_WIDTH = 0.002f;
        // how much to offset display from its original position
        protected static SCNVector3 DISPLAY_OFFSET = new SCNVector3(-0.2f, 0.3f, 0);
        // displays can't be made more than this distance away from user.
        protected const Single DISPLAY_DISTANCE_CLAMP = 2.0f; 
        // just a useful value for 90 degrees.
        protected const Single NINETY = (Single)Math.PI / 2;


        static PerceptQRViewController()
        {
            DISPLAY_MAT[0].Diffuse.Contents = DISPLAY_COLOR;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Debug.Print("PerceptQRViewController ViewDidLoad()");
            //make the text empty (but keep the text Label in storyboard for easy editing
            HelpText.Text = "Loading...";
            SelectText.Text = "";
            // just incase order is screwed up in the storyboard.
            snView.BringSubviewToFront(HelpText);
            snView.BringSubviewToFront(SelectText);
            snView.BringSubviewToFront(ActivityIndicator);
            snView.BringSubviewToFront(OKButton);
            snView.BringSubviewToFront(CancelButton);


            ActivityIndicator.Hidden = true;
            HideClassificationPicker();
            pickerModel = new ClassificationPickerModel(
                ((VNClassificationObservation observation) => ClassificationPickedHandler(observation)), CLASSIFICATIONS_SIZE);
            ClassificationPicker.Model = pickerModel;


            //respond to object events
            virtualObjectManager.VirtualObjectManagerDelegate = this;
            displayPoints = new Dictionary<string, Point>();
            displayLines = new Dictionary<string, SCNNode>();

            serialImageQueue = new DispatchQueue(label: "com.xamarin.Percept.serialImageQueue");

            //this will be called async
            qrRequest = new FixVNDetectBarcodesRequest(QRRequestHanlder);
            qrRequest.Symbologies = symbologies;

            // load the pretrained model
            var assetPath = NSBundle.MainBundle.GetUrlForResource("MobileNet", "mlmodelc");
            imageMLModel = MLModel.Create(assetPath, out NSError mlError);
            if(mlError != null)
            {
                Debug.Print("Couldn't load MLModel\n"+mlError.DebugDescription);
                GoBack();//TODO behavior with respect to callstack of viewdidload?
            }
            imageVNModel = VNCoreMLModel.FromMLModel(imageMLModel, out NSError vnError);
            if (vnError != null)
            {
                Debug.Print("Couldn't load VNCoreMLModel\n" + vnError.DebugDescription);
                GoBack();//TODO behavior with respect to callstack of viewdidload?
            }
            mlRequest = new VNCoreMLRequest(imageVNModel, MLRequestHandler);
            // TODO haven't tried scale fit
            mlRequest.ImageCropAndScaleOption = VNImageCropAndScaleOption.CenterCrop;

            serialSceneQ.DispatchAsync(() => associations = store.GetAssociations());
        }

        public override UILabel GetMessageLabel()
        {
            return HelpText;
        }


        //clean up any resources and pop view controller.
        protected void GoBack()
        {
            //go back to menu
            NavigationController.PopViewController(false);
        }

        partial void OKPressed(UIButton sender)
        {
            Debug.Print("OKPressed()");
            VNClassificationObservation obs = pickerModel.GetSelectedClassification(ClassificationPicker);
            ClassificationPickedHandler(obs);

        }

        partial void CancelPressed(UIButton sender)
        {
            Debug.Print("CancelPressed()");
            HideClassificationPicker();
        }


        partial void BackPressed(UIButton sender)
        {
            Debug.Print("BackPressed()");
            GoBack();
        }

        public void NothingTapped()
        {
            Debug.Print("PerceptQRViewController NothingTapped()");
            if (selectedDisplayId != null)
            {
                ShowClassificationPicker();
            }
        }

        public void ObjectTapped(VirtualObject virtualObject)
        {
            var sensorDisplay = virtualObject as SensorDisplay;
            if (sensorDisplay != null)
            {
                selectedDisplayId = sensorDisplay.Name;
                SetSelectionText(selectedDisplayId);
            }
        }


        protected void HideClassificationPicker()
        {
            ClassificationPicker.Hidden = true;
            OKButton.Hidden = true;
            CancelButton.Hidden = true;
            ClassificationPickerBackground.Hidden = true;
        }

        // user is making a binding from display to classification object
        protected void ShowClassificationPicker()
        {
            pickerModel.SetDisplayedClassifications(lastClassifications);
            ClassificationPicker.ReloadAllComponents();
            ClassificationPicker.Hidden = false;
            OKButton.Hidden = false;
            CancelButton.Hidden = false;
            ClassificationPickerBackground.Hidden = false;
        }


        // show the picker and resample its internal data from current classifications
        protected void ClassificationPickedHandler(VNClassificationObservation obs)
        {
            if(obs == null)
            {
                Debug.Print("ClassificationPickedHandler(VNClassificationObservation obs) obs can't be null");
                return;
            }
            if(selectedDisplayId  == null)
            {
                Debug.Print("ClassificationPickedHandler(VNClassificationObservation obs) selectedDisplayId can't be null");
                return;
            }
            Debug.Print("Picked " + obs.Identifier);
            serialSceneQ.DispatchAsync(() => 
            {
                associations.AddAssociation(selectedDisplayId, obs.Identifier, obs.Confidence);
                serialSceneQ.DispatchAsync(() => store.SetAssociations(associations));
                DisplayNewObjectMapping(selectedDisplayId);
                selectedDisplayId = null;
            });
            SelectText.Text = "";
            HideClassificationPicker();
        }

        // GC.KeepAlive(currFrame); is used to keep other threads from deleting this frame until it is not needed.
        // on the other hand if we don't dispose of it soon enough, our application doesn't render.
        protected ARHitTestResult FeatureHitTestFromPoint(CGPoint point)
        {
            if(currFrame == null || point == null)
            {
                return null;
            }
            ARHitTestResult[] hitTestResults = null;
            try
            {

                hitTestResults = currFrame.HitTest(point,
                ARHitTestResultType.FeaturePoint | ARHitTestResultType.ExistingPlaneUsingExtent
                | ARHitTestResultType.EstimatedHorizontalPlane);
                if (hitTestResults != null && hitTestResults.Length > 0)
                {
                    return hitTestResults[0];
                }
                GC.KeepAlive(currFrame);
                return null;
            }
            catch(Exception e)
            {
                Debug.Print("FeatureHitTestFromPoint " + e.Message);
                return null;
            }
        }

        protected void SetSelectionText(string sensorId)
        {
            string text = "Selected: " + sensorId;
            associations.SensorIdToObject.TryGetValue(sensorId, out Tuple<string, float> objectName);
            if(objectName != null)
            {
                text = text + "->" + objectName.Item1;
            }
            SelectText.Text = text;
        }

        SCNNode OffsetLine(SensorDisplay sensor, Point point, SCNNode lineNode = null)
        {
            SCNCapsule geom = null;
            if (lineNode == null)
            {
                lineNode = new SCNNode();
                geom = new SCNCapsule();

                geom.CapRadius = LINE_WIDTH;
                geom.Materials = DISPLAY_MAT;
                lineNode.Geometry = geom;

            }else
            {
                geom = lineNode.Geometry as SCNCapsule;
            }

            SCNVector3 backPos = sensor.ConvertPositionToNode(new SCNVector3(
                0, -(SensorDisplay.HEIGHT / 2), LINE_WIDTH/2), scene.RootNode);
            SCNVector3 delta = SCNVector3.Subtract(backPos, point.WorldPosition);
            Single distance = (Single)Math.Sqrt((delta.X * delta.X) + (delta.Y * delta.Y) + (delta.Z * delta.Z));
            SCNVector3 mid = new SCNVector3(delta.X / 2, delta.Y / 2, delta.Z / 2);

            geom.Height = distance;
            
            lineNode.WorldPosition = SCNVector3.Add(point.WorldPosition, mid);

            delta.Normalize();
            Single ang = SCNVector3.CalculateAngle(new SCNVector3(0f, 1f, 0f), delta);
            //(x, y, z) x (x, 0, z) == (y z, 0, -x y) faster cross product under y = 0 assumption 
            SCNVector3 quickCross = new SCNVector3(delta.Y * delta.Z, 0, -(delta.X * delta.Y));
            if(ang < -NINETY || ang > NINETY)
            {
                quickCross.X = -quickCross.X;
                quickCross.Z = -quickCross.Z;
            }
            SCNQuaternion q = SCNQuaternion.FromAxisAngle(quickCross, ang);
            lineNode.Orientation = SCNQuaternion.Identity;
            lineNode.LocalRotate(q);

            return lineNode;
        }

        // GC.KeepAlive(currFrame); is used to keep other threads from deleting this frame until it is not needed.
        // on the other hand if we don't dispose of it soon enough, our application doesn't render.
        protected void DisplayNewObjectMapping(string sensorId)
        {
            Debug.Print("DisplayNewObjectMapping");
            if (idToSensorDisplay.TryGetValue(sensorId, out SensorDisplay sensor))
            {
                ARHitTestResult centerHit = null;
                try
                {
                    if(currFrame != null)
                    {
                        centerHit = FeatureHitTestFromPoint(snView.Bounds.GetMidpoint());
                        GC.KeepAlive(currFrame);
                    }
                    
                }
                catch (Exception e)
                {
                    Debug.Print("DisplayNewObjectMapping exception when hit testing " + e.Message);
                }

                if (centerHit != null)
                {
                    

                    SCNVector3 pos = centerHit.WorldTransform.Translation();
                    Debug.Print("pos: " + pos.ToString());
                    Debug.Print("distance: " + centerHit.Distance);
                    if (displayPoints.TryGetValue(sensorId, out Point point))
                    {
                        Debug.Print("displayPoints had point with world pos: " + point.WorldPosition);
                        point.RemoveFromParentNode();
                        displayPoints.Remove(sensorId);
                        point.Dispose();
                    }
                    if (displayLines.TryGetValue(sensorId, out SCNNode line))
                    {
                        Debug.Print("displayLines had line with world pos: " + line.WorldPosition);
                        line.RemoveFromParentNode();
                        displayLines.Remove(sensorId);
                        line.Dispose();
                    }
                    Single clampedDistance = Math.Min((Single)centerHit.Distance, DISPLAY_DISTANCE_CLAMP);
                    SCNVector3 cameraDistance = new SCNVector3(0, 0, -clampedDistance);
                    SCNMatrix4 distanceMatrix = SCNMatrix4.CreateTranslation(cameraDistance);
                    Point newPoint = new Point();
                    PlaceInFrontOfCamera(newPoint, distanceMatrix);

                    displayPoints[sensorId] = newPoint;
                    scene.RootNode.AddChildNode(newPoint);

                    SCNNode newLine = OffsetLine(sensor, newPoint);
                    displayLines[sensorId] = newLine;
                    scene.RootNode.AddChildNode(newLine);
                }
                else
                {
                    Debug.Print("centerHit == null");
                    serialSceneQ.DispatchAsync(() => DisplayNewObjectMapping(sensorId));
                }
            }
            else
            {
                Debug.Print("sensorId not found in idToSensorDisplay in DisplayNewObjectMapping");
            }

        }
        protected void GetDisplayContentsForSensor(SensorDisplay sensor, string sensorId)
        {
            UIImage image = store.GetPlot(sensorId);


            Point point = new Point();
            displayPoints[sensorId] = point;
            point.WorldTransform = sensor.WorldTransform;
            Debug.Print("GetDisplayContentsForSensor point.WorldTransform\n" + point.WorldTransform.ToString());
            scene.RootNode.AddChildNode(point);
            sensor.LocalTranslate(DISPLAY_OFFSET);

            SCNNode lineNode = OffsetLine(sensor, point);
            displayLines[sensorId] = lineNode;
            scene.RootNode.AddChildNode(lineNode);

            sensor.SetContents(image);
            sensor.Hidden = false;
            loadingImage = false;
            selectedDisplayId = sensorId;
            virtualObjectManager.AddVirtualObject(sensor);
            InvokeOnMainThread(() => {
                this.ActivityIndicator.Hidden = true;
                SetSelectionText(sensorId);
                Debug.PrintWT("GetDisplayContentsForSensor Finish for " + sensorId);
            });
        }

        protected void GetDisplayForCode(string codeId, CGPoint center,
            CGPoint bottomLeft, CGPoint bottomRight, CGPoint topLeft, CGPoint topRight)
        {
                // Perform a hit test on the ARFrame to find a surface
                ARHitTestResult centerHit = FeatureHitTestFromPoint(center);
                ARHitTestResult bottomLeftHit = FeatureHitTestFromPoint(bottomLeft);
                ARHitTestResult bottomRightHit = FeatureHitTestFromPoint(bottomRight);
                ARHitTestResult topLeftHit = FeatureHitTestFromPoint(topLeft);
                ARHitTestResult topRightHit = FeatureHitTestFromPoint(topRight);

                if (centerHit != null && bottomLeftHit != null && bottomRightHit != null
                    && topLeftHit != null && topRightHit != null)
                {
                    if (centerHit.Distance < QRCODE_DISTANCE_THRESH
                        && bottomLeftHit.Distance < QRCODE_DISTANCE_THRESH
                        && bottomRightHit.Distance < QRCODE_DISTANCE_THRESH
                        && topLeftHit.Distance < QRCODE_DISTANCE_THRESH
                        && topRightHit.Distance < QRCODE_DISTANCE_THRESH)
                    {
                        Single clampedDistance = Math.Min((Single)centerHit.Distance, DISPLAY_DISTANCE_CLAMP);
                        PlaceDisplayFromCameraTransform(codeId, clampedDistance);
                    }
                }
        }


        protected void LoadIfPossible(SensorDisplay display, string codeId)
        {

            Debug.PrintWT("LoadIfPossible (before invoke on main)" + codeId);
            InvokeOnMainThread(() =>
            {
                if (!loadingImage)
                {
                    loadingImage = true;
                    this.ActivityIndicator.Hidden = false;
                    serialSceneQ.DispatchAsync(() => GetDisplayContentsForSensor(display, codeId));
                }
            });
        }

        protected void PlaceDisplayFromCameraTransform(string codeId, Single distance)
        {
            if(currFrame?.Camera != null && snView?.PointOfView != null)
            {
                //distance from camera
                SCNVector3 cameraDistance = new SCNVector3(0, 0, -distance);
                SCNMatrix4 distanceMatrix = SCNMatrix4.CreateTranslation(cameraDistance);
                SensorDisplay display = new SensorDisplay(codeId);
                display.Hidden = true;
                scene.RootNode.Add(display);
                SetSensorDisplayPositionFromCamera(display, distanceMatrix);

                if (!idToSensorDisplay.ContainsKey(codeId))
                {
                    Debug.Print("New code found: " + codeId);
                    idToSensorDisplay.Add(codeId, display);
                    LoadIfPossible(display, codeId);

                }
                else
                {
                    SensorDisplay oldDisplay = idToSensorDisplay[codeId];
                    SCNVector3 oldDisplayPos = oldDisplay.WorldPosition;
                    SCNVector3 newDisplayPos = display.WorldPosition;
                    Single posSum = SCNVector3Extensions.SquaredDifferenceSum(ref oldDisplayPos, ref newDisplayPos);
                    if (posSum > CAMERA_DIFFERENCE_THROWAWAY_THRESH)
                    {
                        Debug.Print("Existing code found: " + codeId + " and had a new display generated. posSum: " + posSum);
                        oldDisplay.WorldTransform = display.WorldTransform;
                    }
                    else
                    {
                        Debug.Print("Existing code found: " + codeId + " and failed to meet tresh posSum: " + posSum);
                    }
                    LoadIfPossible(oldDisplay, codeId);
                    display.RemoveFromParentNode();
                }
            }else
            {
                Debug.Print("The ARFrame or Camera or scene or camera node were null in PlaceDisplayFromCameraTransform");
            }
            
        }




        public void CouldNotPlace(VirtualObject virtualObject)
        {
            //throw new NotImplementedException();
        }
        public void TranslationFinishedFor(VirtualObject virtualObject)
        {
            //Redo line
            SensorDisplay sensor = virtualObject as SensorDisplay;
            if (sensor != null)
            {
                //if we don't have a point or a sensor we can't do anything.
                if (displayPoints.TryGetValue(sensor.Name, out Point point))
                {
                    if (displayLines.TryGetValue(sensor.Name, out SCNNode line))
                    {
                        OffsetLine(sensor, point, line);
                        line.Hidden = false;
                    }

                }
            }
        }

        public void TransformDidChangeFor(VirtualObject virtualObject)
        {
            //hide the line while it's being moved
            SensorDisplay sensor = virtualObject as SensorDisplay;
            if (sensor != null)
            {
                if (displayLines.TryGetValue(sensor.Name, out SCNNode line))
                {
                    line.Hidden = true;
                }
            }
        }



        

        protected void MLRequestHandler(VNRequest request, NSError error)
        {
            if (error != null)
            {
                Debug.Print(error.DebugDescription);
            }else
            {
                VNCoreMLRequest req = request as VNCoreMLRequest;
                if(req == null)
                {
                    Debug.Print("VNCoreMLRequest was null in handler");
                    return;
                }

                var results = req.GetResults<VNClassificationObservation>();
                int end = results.Length;
                if(end != 0)
                {
                    int i = 0;
                    for(; i<CLASSIFICATIONS_SIZE && i!=end; ++i)
                    {
                        lastClassifications.Array[i] = results[i];
                    }
                    lastClassifications.Used = i;
                }
            }
        }
        protected void QRRequestHanlder(VNRequest request, NSError error)
        {
            if (error != null)
            {
                Debug.Print(error.DebugDescription);
            }
            else
            {
                VNDetectBarcodesRequest barcodeRequest = request as VNDetectBarcodesRequest;
                if (barcodeRequest == null)
                {
                    Debug.Print("VNDetectBarcodesRequest was null in handler");
                    return;
                }
                var results = barcodeRequest.GetResults<VNBarcodeObservation>();
                if (results.Length > 0)
                {
                    VNBarcodeObservation result = results[0];
                    CGPoint center = CGPointExtensions.MidPoint(result.BottomLeft, result.TopRight);

                    //Debug.Print("center : " + center);
                    serialSceneQ.DispatchAsync(() =>
                        GetDisplayForCode(result.PayloadStringValue, center,
                            result.BottomLeft, result.BottomRight, result.TopLeft, result.TopRight)
                    );
                }
            }
        }

        //IARSCNViewDelegate inherits from SCNSceneRendererDelegate
        [Export("renderer:updateAtTime:")]
        public void RendererUpdateAtTime(SCNSceneRenderer renderer, double updateAtTime)
        {
            if(lastClassifications.Used > 0 && associations != null)
            {
                for(int i = 0, end = lastClassifications.Used; i!= end; ++i)
                {
                    VNClassificationObservation obs = lastClassifications.Array[i];
                    if (associations.ObjectToSensorId.TryGetValue(obs.Identifier, out Tuple<string, float> sensor))
                    {
                        //we have an association. Is it displayed already?
                        //TODO better positioning?
                        string sensorId = sensor.Item1;
                        if (!idToSensorDisplay.ContainsKey(sensorId) && (obs.Confidence + CONFIDENCE_DELTA) >= sensor.Item2)
                        {
                            Debug.Print("Found object " + obs.Identifier + " with id " + sensorId);
                            try
                            {
                                    ARHitTestResult centerHit = FeatureHitTestFromPoint(snView.Bounds.GetMidpoint());
                                    if (centerHit != null)
                                    {
                                        if (hitDistanceAverager.AddSampleGetMedian(obs.Identifier, centerHit, out HitDistanceAverager.HitObject hit))
                                        {
                                            Single clampedDistance = Math.Min((Single)centerHit.Distance, DISPLAY_DISTANCE_CLAMP);
                                            serialSceneQ.DispatchAsync(() => PlaceDisplayFromCameraTransform(sensorId, clampedDistance));
                                        }
                                    }
                            }
                            catch(Exception e)
                            {
                                Debug.Print("FeatureHitTestFromPoint(snView.Bounds.GetMidpoint()) exception\n" + e.Message);
                            }
                            

                        }
                    }
                }
            }

            if (currFrame == null || currPixelbuff == null)
            {
                return;
            }
            
            if(!analyzingFrame)
            {
                analyzingFrame = true;
                serialImageQueue.DispatchAsync(() =>
                {
                    VNImageRequestHandler handler = null;
                    try
                    {
                        //some exception happens randomly...
                        handler = new VNImageRequestHandler(currPixelbuff, new NSDictionary());
                        NSError nserror;
                        handler.Perform(new VNRequest[]{ qrRequest, mlRequest}, out nserror);
                        if (nserror != null)
                        {
                            Debug.Print(nserror.DebugDescription);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Print("Exception occured when making a VNImageRequestHandler...");
                        Debug.Print(e.Message);
                    }
                    finally
                    {
                        if (handler != null)
                        {
                            handler.Dispose();
                        }
                        analyzingFrame = false;
                    }
                });
            }
        }

        
    }
}

