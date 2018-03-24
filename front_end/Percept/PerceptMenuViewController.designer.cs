// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace Percept
{
    [Register ("PerceptMenuViewController")]
    partial class PerceptMenuViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView MenuView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (MenuView != null) {
                MenuView.Dispose ();
                MenuView = null;
            }
        }
    }
}