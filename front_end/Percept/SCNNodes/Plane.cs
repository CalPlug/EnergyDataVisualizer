using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARKit;
using Foundation;
using SceneKit;
using UIKit;

namespace Percept.SCNNodes
{
    //a horizontal plane for debuggin visualization, and for room mapping 
    public class Plane : SCNNode
    {

        public const string NAME_SYNC = "sync";
        public const string NAME_OBSERVED = "obs";


        public enum PlaneColor
        {
            Blue,
            Green
        }

        //plane was facing me
        //static private SCNQuaternion INIT_ROTATION = new SCNQuaternion(0.707f, -0.707f, 0f, 0f);
        //plane was away from me (backface culled)
        //static private SCNQuaternion INIT_ROTATION = new SCNQuaternion(0f, 0f, 0f, 0f);
        //plane was right orientation, but was upside down (culled)
        //static private SCNQuaternion INIT_ROTATION = new SCNQuaternion(0.707f, 0f, 0f, 0.707f);
        static private SCNQuaternion INIT_ROTATION = new SCNQuaternion(0.707f, 0f, 0f, -0.707f);
        static private SCNVector3 INIT_TARGET = new SCNVector3(0f, 0f, 0f);
        static private SCNMaterial[] MAT_BLUE = new SCNMaterial[] { new SCNMaterial() };
        static private SCNMaterial[] MAT_GREEN = new SCNMaterial[] { new SCNMaterial() };
        static Plane()
        {
            MAT_BLUE[0].Diffuse.Contents = UIImage.FromBundle("Plane");
            MAT_GREEN[0].Diffuse.Contents = UIImage.FromBundle("PlaneGreen");
        }
        private SCNPlane geom;

        private Single width;
        private Single length;

        public ARPlaneAnchor Anchor { get; set; } 

        public Single Width
        {
            get
            {
                return width;
            }
            set
            {
                geom.Width = value;
                width = value;
            }
        }

        public Single Length
        {
            get
            {
                return length;
            }
            set
            {
                geom.Height = value;
                length = value;
            }
        }

        public Plane(Single width, Single length, PlaneColor color = PlaneColor.Blue, bool initialRotate = true)
        {
            //floats like width and length can't be null
            geom = new SCNPlane();
            Width = width;
            Length = length;
            switch (color)
            {
                case PlaneColor.Blue:
                    geom.Materials = MAT_BLUE;
                    break;
                case PlaneColor.Green:
                    geom.Materials = MAT_GREEN;
                    break;
                default:
                    geom.Materials = MAT_BLUE;
                    break;
            }
            
            this.Geometry = geom;
            if (initialRotate)
            {
                this.Rotate(INIT_ROTATION, INIT_TARGET);
            }
        }

        public void SetColor(PlaneColor color = PlaneColor.Blue)
        {
            switch (color)
            {
                case PlaneColor.Blue:
                    geom.Materials = MAT_BLUE;
                    break;
                case PlaneColor.Green:
                    geom.Materials = MAT_GREEN;
                    break;
                default:
                    geom.Materials = MAT_BLUE;
                    break;
            }
        }
    }
}