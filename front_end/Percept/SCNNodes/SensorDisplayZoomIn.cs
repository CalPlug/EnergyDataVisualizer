using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using SpriteKit;
using CoreGraphics;
using Percept.Utility;

namespace Percept.SCNNodes
{
    public class SensorDisplayZoomIn : SKNode
    {

        protected SKSpriteNode plot;
        protected SKTexture texture;

        protected static CGSize FULL_SIZE = new CGSize(1.0, 1.0);
        protected static CGSize PLOT_SIZE = new CGSize(1.0, 0.90);
        protected static CGPoint BACKGROUND_POS = new CGPoint(0.5, 0.5);
        protected static CGPoint PLOT_POS = new CGPoint(0.5, 0.54);

        protected SKSpriteNode BACKGROUND = 
            SKSpriteNode.FromColor(UIColor.FromRGBA(190, 255, 255, 255), FULL_SIZE);
        protected Action OnTouchesBeganAction;

        public SensorDisplayZoomIn(Action onTouchesBeganAction)
        {
            UserInteractionEnabled = true;

            AddChild(BACKGROUND);
            BACKGROUND.Position = BACKGROUND_POS;
            BACKGROUND.ZPosition = -1;
            OnTouchesBeganAction = onTouchesBeganAction;
        }

        [Export("touchesBegan:withEvent:")]
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            Debug.Print("SensorDisplayZoomIn TouchesBegan");
            OnTouchesBeganAction.Invoke();
        }

        public void SetContents(UIImage image)
        {
            if(plot != null)
            {
                plot.RemoveFromParent();
            }

            texture = SKTexture.FromImage(image);
            plot = SKSpriteNode.FromTexture(texture, PLOT_SIZE);
            AddChild(plot);
            plot.Position = PLOT_POS;
        }


    }
}