using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using Percept.ObjectExtensions;
using UIKit;
using Vision;

namespace Percept.UIElements
{
    // Apple class for managing the data and appearence of the items inside the picker for classifications
    public class ClassificationPickerModel : UIPickerViewModel
    {

        public Action<VNClassificationObservation> OnSelectedObservation { get; set; }

        //if we just keep a reference to the classifications in the qr controller, the list will change as we scroll.
        protected FixedArray<VNClassificationObservation> DisplayedClassifications;

        public ClassificationPickerModel(Action<VNClassificationObservation> action, int size)
        {
            OnSelectedObservation = action;
            DisplayedClassifications = new FixedArray<VNClassificationObservation>(size);
        }

        public void SetDisplayedClassifications(FixedArray<VNClassificationObservation> classifications)
        {
            for(int i = 0, end = classifications.Used; i!= end; ++i)
            {
                DisplayedClassifications.Array[i] = classifications.Array[i];
            }
            DisplayedClassifications.Used = classifications.Used;
        }

        public override nint GetComponentCount(UIPickerView v)
        {
            return 1;
        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return DisplayedClassifications.Used;
        }

        public override string GetTitle(UIPickerView picker, nint row, nint component)
        {
            return DisplayedClassifications.Array[row].Identifier;
        }

        public override void Selected(UIPickerView picker, nint row, nint component)
        {
            OnSelectedObservation.Invoke(DisplayedClassifications.Array[row]);
        }

        public VNClassificationObservation GetSelectedClassification(UIPickerView picker)
        {
            nint row = picker.SelectedRowInComponent(0);
            return DisplayedClassifications.Array[row];
        }

        //public override nfloat GetComponentWidth(UIPickerView picker, nint component)
        //{
        //    if (component == 0)
        //        return 240f;
        //    else
        //        return 40f;
        //}

        //public override nfloat GetRowHeight(UIPickerView picker, nint component)
        //{
        //    return 40f;
        //}
    }
}