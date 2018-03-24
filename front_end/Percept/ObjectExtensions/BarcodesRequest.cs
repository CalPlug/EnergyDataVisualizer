using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using Vision;

namespace Percept.ObjectExtensions
{
    //fix xamarin bug for using this interface. (something is unbound in system, at somepoint they could fix it.)
    public class FixVNDetectBarcodesRequest : VNDetectBarcodesRequest
    {
        public FixVNDetectBarcodesRequest(NSObjectFlag t) : base(t) { }

        public FixVNDetectBarcodesRequest(IntPtr handle) : base(handle) { }

        public FixVNDetectBarcodesRequest(VNRequestCompletionHandler completionHandler) : base(completionHandler) { }
    }
}
