// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Percept
{
    [Register ("PerceptQRViewController")]
    partial class PerceptQRViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIActivityIndicatorView ActivityIndicator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton CancelButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIPickerView ClassificationPicker { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView ClassificationPickerBackground { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel HelpText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton OKButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel SelectText { get; set; }

        [Action ("BackPressed:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void BackPressed (UIKit.UIButton sender);

        [Action ("CancelPressed:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void CancelPressed (UIKit.UIButton sender);

        [Action ("OKPressed:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void OKPressed (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (ActivityIndicator != null) {
                ActivityIndicator.Dispose ();
                ActivityIndicator = null;
            }

            if (CancelButton != null) {
                CancelButton.Dispose ();
                CancelButton = null;
            }

            if (ClassificationPicker != null) {
                ClassificationPicker.Dispose ();
                ClassificationPicker = null;
            }

            if (ClassificationPickerBackground != null) {
                ClassificationPickerBackground.Dispose ();
                ClassificationPickerBackground = null;
            }

            if (HelpText != null) {
                HelpText.Dispose ();
                HelpText = null;
            }

            if (OKButton != null) {
                OKButton.Dispose ();
                OKButton = null;
            }

            if (SelectText != null) {
                SelectText.Dispose ();
                SelectText = null;
            }
        }
    }
}