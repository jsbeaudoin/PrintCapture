using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using PrintsCapture.Prints.Enum;
using XL_ID.Utilities.Wpf.WindowHelper;
using PrintsCapture.LocalAwSeqCheck;
using PrintsCapture.Prints;
using PrintsCapture.Prints.Language;
using PrintsCapture.Prints.Sequence;
using PrintsCapture.Ui.Language;
using PrintsCapture.Ui.Print;

namespace PrintsCapture.Ui.Class.Wizard
{
    using PrintsCapture.Prints.Extension;

    using UniBIO.Services.Communication.BiometricService;

    public class PrintCaptureWizard
    {
        private LocalSeqCheck sequenceCheckService;
        private CapturedPrintData result = null;

        /// <summary>
        /// Display PrintsCapture Wizard dialog for Livescan
        /// </summary>
        /// <param name="lang">2 letters iso code of the language. fr or en.</param>
        /// <param name="captureGroup">Capture group for sequence. 2 = FlatOnly. 4 = Flats and rolled. 8 = Single finger only.</param>
        /// <param name="debug">Indicate that application runs in debug mode, enabling virtual scanners.</param>
        /// <returns>WizardCaptureResult instance with the resulting prints</returns>
        public CapturedPrintData ShowWizardDialog(string lang, int captureGroup, bool debug)
        {
            try
            {
                // make sure that winform or wpf has an application !
                if (null == System.Windows.Application.Current)
                {
                    var a = new System.Windows.Application();
                    a.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                }

                var cult = new CultureInfo(lang);
                CommonText.Culture = cult;
                Text.Culture = cult;

                var param = new PrintCaptureAppParameter
                {
                    CultureName = lang,
                    IsWizardMode = true,
                    DescriptionLine1 = string.Empty,
                    DescriptionLine2 = string.Empty,
                    IcdVersion = @"178",
                    Mode = @"livescan",
                    IsOptionAvailable = false,
                    CaptureModeAllowed = (PrintCaptureGroup)captureGroup, // .FlatOnly
                    IsEndorsementAllowed = true,
                    IsDebugAvailable = debug
                };

                PrintCaptureApp.ApplicationCulture = new CultureInfo(param.CultureName);

                this.sequenceCheckService =
                    new LocalSeqCheck(
                        CaptureKind.Livescan,
                        param.PrintList);

                param.SeqCheckService = sequenceCheckService;
                
                var app = PrintCaptureApp.Start(param);

                // Rules are loaded in the Application start, they must be set after, else they could be overwritten
                var rules = param.PrintList.Rules;
                rules.SequenceThreshold = 50;
                rules.IsSequenceEnabled = true;
                rules.IsQualityEnabled = false;

                if (app != null)
                {
                    app.ScanCompleted += AppOnScanCompleted;
                }

                PrintCaptureApp.OpenMainFormDialog();

                return result;

                //splash.Close();

            }
            catch (Exception ex)
            {
                throw new  ApplicationException(@"Cannot start : " + ex.Message);                
            }
        }

        private void AppOnScanCompleted(object sender, ScanCompletedEventArgs e)
        {
            if (PrintCaptureApp.HasWizardAcceptedPrint)
            {
                this.result = this.sequenceCheckService.GetCapturedPrintData(); //  new WizardCaptureResult(this.sequenceCheckService);
            }
            
        }
        
    }    
}
