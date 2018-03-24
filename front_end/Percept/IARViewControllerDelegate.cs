using ARKit;
using UIKit;
using Percept.Utility;
using SceneKit;
using Foundation;
using System;

namespace Percept
{
    public interface IARViewControllerDelegate
    {
        ARFrame GetCurrentFrame();

        SCNView GetView();

        void RestartExperience(NSObject sender);

        UILabel GetMessageLabel();

        //TODO if something implements UIViewController, it should automatically implement this function?
        void PresentViewController(UIViewController viewControllerToPresent, bool animated, Action completionHandler);
    }
}