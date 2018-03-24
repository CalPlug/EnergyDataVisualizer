using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Percept.Model
{
    // wraps the SensorObjectAssociation class so its usable by the db.
    public class AssociationsWrapper
    {
        // for now - we just say there is one room
        public string uid { get; set; } = "room1";
        // the arbitrary json string of the SensorObjectAssociation object.
        public string data { get; set; }
    }
}