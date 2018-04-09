using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using SpriteKit;
using CoreGraphics;

namespace Percept.SCNNodes
{
    public class SensorDisplayZoomIn : SKNode
    {

        protected SKSpriteNode plot;
        protected SKTexture texture;
        protected static CGSize size = new CGSize(1.0, 1.0);
        public SensorDisplayZoomIn()
        {
            UserInteractionEnabled = true;
        }

        public void SetContents(UIImage image)
        {
            if(plot != null)
            {
                plot.RemoveFromParent();
                plot.Dispose();
            }

            if(texture != null)
            {
                texture.Dispose();
            }

            texture = SKTexture.FromImage(image);
            plot = SKSpriteNode.FromTexture(texture, size);
            AddChild(plot);
        }
    }
}