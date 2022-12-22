using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintsCapture.Ui.Class
{
    using System.Windows;

    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Extension;
    using PrintsCapture.Prints.Language;
    using PrintsCapture.Ui.View;
    using PrintsCapture.Ui.ViewModel;
    

    public class PalmUserValidationService
    {


        public void VerifyQuality(PrintList printList)
        {
            var upperPalms = printList.Prints.Where(x => x.HandPart == HandPart.UpperPalm).ToList();

            foreach (var upperPalm in upperPalms)
            {
                if (upperPalm.IsAcceptedByUser)
                {
                    continue;
                }

                if (upperPalm.Image != null && upperPalm.Status == PrintStatus.Validated
                    && upperPalm.FailedValidations.Contains(PrintError.SequenceError))
                {
                    var vm = new PalmQualityControlViewModel();
                    vm.PalmImage = printList.GetImage(upperPalm);
                    vm.Accepted = null;
                    vm.Message = string.Format(CommonText.PalmQualityIsLow, PrintList.GetName(upperPalm));

                    var win = new PalmQualityControlWindow(vm);
                    win.ShowDialog();

                    if (vm.Accepted == null)
                    {
                        this.VerifyQuality(printList);
                        return;
                    }

                    if (vm.Accepted == false)
                    {
                        PrintInfo palm = upperPalm;
                        Application.Current.Dispatcher.BeginInvoke(
                            new Action(
                                () => PrintCaptureDriver.Instance.CaptureSingle(palm)));
                        
                        return;
                    }

                    upperPalm.IsAcceptedByUser = true;

                }

            }


        }

    }
}
