using SceneKit;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Percept.SCNNodes
{
    // Our plot container
    public class SensorDisplay : VirtualObject
    {
        public const Single WIDTH = 0.40096f;
        public const Single HEIGHT = 0.30072f;

        static protected SCNMaterial[] MAT_BACKGROUND= new SCNMaterial[] { new SCNMaterial() };
        //the background color which is behind the plot.
        protected const Single BACKGROUND_WIDTH = WIDTH;// + .01f;
        protected const Single BACKGROUND_HEIGHT = HEIGHT + .01f;
        protected SCNNode background = null;
        static protected SCNVector3 BACKGROUND_OFFSET = new SCNVector3(0, 0, -0.001f);

        static SensorDisplay()
        {
            MAT_BACKGROUND[0].Diffuse.Contents = UIColor.FromRGBA(190, 255, 255, 225);
        }

        protected SCNPlane geom;

        public void SetContents(UIImage image)
        {
            SCNMaterial[]  matContents = new SCNMaterial[] { new SCNMaterial() };
            matContents[0].Diffuse.Contents = image;
            matContents[0].ReadsFromDepthBuffer = false;
            geom.Materials = matContents;
            AddBackground();
        }

        public void UpdateContents(UIImage image)
        {
            geom.Materials[0].Diffuse.Contents = image;
        }

        public SensorDisplay(string id)
        {
            geom = new SCNPlane();
            geom.Width = WIDTH;
            geom.Height = HEIGHT;
            Name = id;
            Geometry = geom;
            RenderingOrder = 2;
        }

        public void AddBackground()
        {
            background = new SCNNode();
            SCNPlane backgroundGeom = new SCNPlane();
            backgroundGeom.Width = BACKGROUND_WIDTH;
            backgroundGeom.Height = BACKGROUND_HEIGHT;
            backgroundGeom.Materials = MAT_BACKGROUND;
            background.Geometry = backgroundGeom;
            background.LocalTranslate(BACKGROUND_OFFSET);
            AddChildNode(background);
        }

        public void RemoveBackground()
        {
            background.RemoveFromParentNode();
            background = null;
        }


    }
}
