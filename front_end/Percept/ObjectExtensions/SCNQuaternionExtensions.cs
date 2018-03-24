using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using SceneKit;
using UIKit;

namespace Percept.ObjectExtensions
{
    public static class SCNQuaternionExtensions
    {
        //return the (Swing, Twist) SCNQuaternion decompositon
        public static (SCNQuaternion, SCNQuaternion) SwingTwistDecompositon(SCNVector3 unitAxis, SCNQuaternion quat)
        {
            //https://stackoverflow.com/questions/3684269/component-of-a-quaternion-rotation-around-an-axis
            //http://www.euclideanspace.com/maths/geometry/rotations/for/decomposition/
            SCNVector3 rotationAxis = new SCNVector3(quat.X, quat.Y, quat.Z);
            //if we use the formula a proj b =  ( (a dot b) / norm(b)^2 ) * b
            // then we can take a shortcut, since we assume b is a unit vector
            //a = rotationAxis b = unitAxis projection = rotationAxis proj unitAxis

            SCNVector3 projection = SCNVector3.Multiply(unitAxis, SCNVector3.Dot(rotationAxis, unitAxis)) ;
            SCNQuaternion twist = new SCNQuaternion(projection.X, projection.Y, projection.Z, quat.W);
            twist.Normalize();
            SCNQuaternion swing = SCNQuaternion.Multiply(quat , SCNQuaternion.Conjugate(twist));
            return (swing, twist);
        }

        //return the  Twist SCNQuaternion decompositon
        public static SCNQuaternion TwistDecompositon(SCNVector3 unitAxis, SCNQuaternion quat)
        {
            //https://stackoverflow.com/questions/3684269/component-of-a-quaternion-rotation-around-an-axis
            //http://www.euclideanspace.com/maths/geometry/rotations/for/decomposition/
            SCNVector3 rotationAxis = new SCNVector3(quat.X, quat.Y, quat.Z);
            //if we use the formula a proj b =  ( (a dot b) / norm(b)^2 ) * b
            // then we can take a shortcut, since we assume b is a unit vector
            //a = rotationAxis b = unitAxis projection = rotationAxis proj unitAxis

            SCNVector3 projection = SCNVector3.Multiply(unitAxis, SCNVector3.Dot(rotationAxis, unitAxis));
            SCNQuaternion twist = new SCNQuaternion(projection.X, projection.Y, projection.Z, quat.W);
            twist.Normalize();
            return twist;
        }

        public static SCNQuaternion RemoveTwistRotation(SCNQuaternion q, SCNVector3 normalizedAxisOfRemoval)
        {
            SCNQuaternion twist = SCNQuaternionExtensions.TwistDecompositon(normalizedAxisOfRemoval, q);

            SCNQuaternion twistConj = SCNQuaternion.Conjugate(twist);

            SCNQuaternion finalOrientation = SCNQuaternion.Multiply(q, twistConj);

            return finalOrientation;
        }

    }
}