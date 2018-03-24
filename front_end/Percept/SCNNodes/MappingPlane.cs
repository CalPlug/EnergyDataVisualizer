using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using Newtonsoft.Json;
using Percept.Model;
using Percept.Utility;
using UIKit;

namespace Percept.SCNNodes
{
    public class MappingPlane : VirtualObject
    {
        private Plane plane;
        public bool IsLocked { get; protected set; } = false;
        public Single? LastLockedWidth { get; protected set; } = null;
        public Single? LastLockedLength { get; protected set; } = null;

        public Single Width
        {
            get
            {
                return plane.Width;
            }
            set
            {
                if (!IsLocked)
                {
                    plane.Width = value;
                }
            }
        }

        public Single Length
        {
            get
            {
                return plane.Length;
            }
            set
            {
                if (!IsLocked)
                {
                    plane.Length = value;
                }
            }
        }

        public MappingPlane(Single width, Single length)
        {
            plane = new Plane(width, length, Plane.PlaneColor.Blue);
            this.AddChildNode(plane);
        }

        public SceneKit.SCNMatrix4 PlaneTransform()
        {
            return plane.Transform;
        }

        public void ToggleLock()
        {
            if (IsLocked) //unlock
            {
                plane.SetColor(Plane.PlaneColor.Blue);
                IsLocked = false;
            }
            else //lock
            {
                LastLockedLength = plane.Length;
                LastLockedWidth = plane.Width;
                plane.SetColor(Plane.PlaneColor.Green);
                IsLocked = true;
            }
        }
    }
}