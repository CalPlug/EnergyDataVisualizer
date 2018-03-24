using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using SceneKit;
using UIKit;

namespace Percept.SCNNodes
{
    //debug visualization of reference origin.
    public class Point : SCNNode
    {
        static private SCNMaterial[] MAT = new SCNMaterial[] { new SCNMaterial() };
        static private SCNSphere geom = SCNSphere.Create(0.005f);
        //static private SCNShape geom;
        //static private SCNVector3 scaleDown = new SCNVector3(0.01f, 0.01f, 0.01f);

        static Point()
        {
            MAT[0].Diffuse.Contents = UIColor.FromRGBA(190, 255, 255, 225);
            //bezier path gets all broken when numbers are too small.
            //UIBezierPath path = UIBezierPath.FromOval(new CGRect(0f, 0f, 0.02f, 0.02f));
            //path.Flatness = 0.0001f;
            //geom = SCNShape.Create(path, 0);
            geom.Materials = MAT;
        }

        public Point()
        {
            Geometry = geom;
            //because UIBezierPath can't make a small circle
            //nvm you can't evens scale it.
            //Scale = scaleDown;
        }


    }
}