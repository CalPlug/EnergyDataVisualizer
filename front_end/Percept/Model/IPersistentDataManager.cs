using System;
using System.Collections.Generic;
using System.Text;

namespace Percept.Model
{
    public interface IPersistentDataManager
    {
        void SetAssociations(SensorAssociations associations);
        SensorAssociations GetAssociations();
        UIKit.UIImage GetPlot(string plotId);
    }
}
