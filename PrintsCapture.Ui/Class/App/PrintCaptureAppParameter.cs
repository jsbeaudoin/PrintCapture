using System;

namespace PrintsCapture.Ui.Class
{
    using NLog;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Sequence;
    using System.Collections.Generic;
    using System.Globalization;

    public class PrintCaptureAppParameter
    {
        private PrintList printList;

        public static PrintCaptureAppParameter FromParameters(Dictionary<string, string> creationArguments)
        {
            Logger logger = LogManager.GetCurrentClassLogger();       
            string fileNamePrefix = string.Empty;
            string langParameter = string.Empty;
            string keyParameter = string.Empty;
            string modeParameter = string.Empty;
            string nameParameter = string.Empty;
            string captureModeParameter = string.Empty;
            string endorsementParameter = string.Empty;
            string singleFingerCaptureLabel = string.Empty;
            string icdVersion = string.Empty;
            bool isWizardMode = false;
            bool debug = false;
            bool isLoginMode = false;
            bool topMostWindow = false;
            bool alwaysCanOverride = false;
            bool sqMode = false;

            logger.Trace("Setting PrintCapture parameters");
            if (creationArguments.ContainsKey("prefix")) fileNamePrefix = creationArguments["prefix"];
            if (creationArguments.ContainsKey("lang")) langParameter = creationArguments["lang"];
            if (creationArguments.ContainsKey("culture")) langParameter = creationArguments["culture"];
            if (creationArguments.ContainsKey("key")) keyParameter = creationArguments["key"];
            if (creationArguments.ContainsKey("name")) nameParameter = creationArguments["name"];
            if (creationArguments.ContainsKey("mode")) modeParameter = creationArguments["mode"];
            if (creationArguments.ContainsKey("capture")) captureModeParameter = creationArguments["capture"];
            if (creationArguments.ContainsKey("endorsement")) endorsementParameter = creationArguments["endorsement"];
            if (creationArguments.ContainsKey("icdversion")) icdVersion = creationArguments["icdversion"];
            if (creationArguments.ContainsKey("wizard")) isWizardMode = creationArguments["wizard"] == "1";
            if (creationArguments.ContainsKey("topmost")) topMostWindow = creationArguments["topmost"] == "1";
            if (creationArguments.ContainsKey("debug")) debug = creationArguments["debug"] == "1";
            if (creationArguments.ContainsKey("login")) isLoginMode = creationArguments["login"] == "1";
            if (creationArguments.ContainsKey("captureorder")) sqMode = creationArguments["captureorder"] == "sq";
            if (creationArguments.ContainsKey("singlecapturemessage")) singleFingerCaptureLabel = creationArguments["singlecapturemessage"];
            if (creationArguments.ContainsKey("alwayscanoverride"))
                alwaysCanOverride = creationArguments["alwayscanoverride"] == "1";
            
            if (string.IsNullOrEmpty(langParameter))
            {
                langParameter = "en"; //English is the default language
            }
            else if (langParameter.Length > 2)
            {
                langParameter = langParameter.Substring(0, 2).ToLowerInvariant();
            }

            try
            {
                PrintCaptureApp.ApplicationCulture = new CultureInfo(langParameter);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Cannot set culture to '{0}'", langParameter);
            }

            int captureMode = 3;
            if (!string.IsNullOrEmpty(captureModeParameter))
            {
                int.TryParse(captureModeParameter, out captureMode);
            }

            var result = new PrintCaptureAppParameter()
            {
                CultureName = langParameter,
                IsWizardMode = isWizardMode,
                DescriptionLine1 = nameParameter,
                DescriptionLine2 = keyParameter,
                IcdVersion = icdVersion,
                Mode = modeParameter == "card" ? "cardscan" : "livescan",
                CaptureOrder = sqMode ? CaptureOrderMode.Sq : CaptureOrderMode.Standard,
                IsOptionAvailable = modeParameter == "card",
                CaptureModeAllowed = (PrintCaptureGroup)captureMode,
                IsEndorsementAllowed = string.IsNullOrEmpty(endorsementParameter) || endorsementParameter == "1",
                IsDebugAvailable = debug,
                IsLoginMode = isLoginMode,
                SingleFingerCapturePrompt = singleFingerCaptureLabel,
                TopMostWindow = topMostWindow,
                AlwaysCanOverrideWizard = alwaysCanOverride
            };
            return result;
        }

        public PrintCaptureAppParameter()
        {
            this.CaptureModeAllowed = PrintCaptureGroup.FlatOnly | PrintCaptureGroup.Standard14;
            this.IsEndorsementAllowed = true;
            //this.printList = new PrintList();
        }

        public string Mode { get; set; }

        public string DescriptionLine1 { get; set; }

        public string DescriptionLine2 { get; set; }

        public string CultureName { get; set; }

        public PrintCaptureGroup CaptureModeAllowed { get; set; }

        public bool IsEndorsementAllowed { get; set; }

        public bool IsOptionAvailable { get; set; }

        public bool IsDebugAvailable { get; set; }

        public bool IsWizardMode { get; set; }

        public bool IsLoginMode { get; set; }

        public bool TopMostWindow { get; set; } 

        public bool AlwaysCanOverrideWizard { get; set; }

        [Obsolete()]
        public string IcdVersion { get; set; }

        public string SingleFingerCapturePrompt { get; set; }

        public PrintList PrintList
        {
            get
            {
                if (this.printList == null)
                {
                    this.printList = new PrintList();
                    CaptureKind captMode;
                    if (Enum.TryParse(this.Mode, true, out captMode))
                    {
                        this.printList.Rules.CaptureKind = captMode;
                    }                    
                }
                
                return this.printList;
            }
        }

        public BaseSeqCheckService SeqCheckService { get; set; }

        public string SeqCheckServiceConnection { get; set; }
        public CaptureOrderMode CaptureOrder { get; set; }
    }
}
