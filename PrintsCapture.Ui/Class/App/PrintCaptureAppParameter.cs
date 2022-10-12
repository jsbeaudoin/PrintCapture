using System;
using System.Drawing;

namespace PrintsCapture.Ui.Class
{
    using NLog;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Sequence;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using UniBIO.Services.Communication.BiometricService;
    using XL_ID.Utilities.Image;
    using XL_ID.Utilities.SevenZip;
    using XL_ID.Utilities.XML;

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
            string previousPrintsSerialized = null;

            logger.Trace("Setting PrintCapture parameters");
            if (creationArguments.ContainsKey("prefix")) fileNamePrefix = creationArguments["prefix"];
            if (creationArguments.ContainsKey("lang")) langParameter = creationArguments["lang"];
            if (creationArguments.ContainsKey("culture")) langParameter = creationArguments["culture"];
            if (creationArguments.ContainsKey("key")) keyParameter = creationArguments["key"];
            if (creationArguments.ContainsKey("name")) nameParameter = creationArguments["name"];
            if (creationArguments.ContainsKey("mode")) modeParameter = creationArguments["mode"];
            
            if (creationArguments.ContainsKey("capture")) captureModeParameter = creationArguments["capture"];
            if (creationArguments.ContainsKey("capturemode")) captureModeParameter = creationArguments["capturemode"];
            if (creationArguments.ContainsKey("endorsement")) endorsementParameter = creationArguments["endorsement"];
            if (creationArguments.ContainsKey("noendorsement") && creationArguments["noendorsement"] == "1") endorsementParameter = "0";

            if (creationArguments.ContainsKey("icdversion")) icdVersion = creationArguments["icdversion"];
            if (creationArguments.ContainsKey("wizard")) isWizardMode = creationArguments["wizard"] == "1";
            if (creationArguments.ContainsKey("topmost")) topMostWindow = creationArguments["topmost"] == "1";
            if (creationArguments.ContainsKey("debug")) debug = creationArguments["debug"] == "1";
            if (creationArguments.ContainsKey("login")) isLoginMode = creationArguments["login"] == "1";
            if (creationArguments.ContainsKey("sqmode")) sqMode = creationArguments["sqmode"] == "1";
            if (creationArguments.ContainsKey("singlecapturemessage")) singleFingerCaptureLabel = creationArguments["singlecapturemessage"];
            if (creationArguments.ContainsKey("alwayscanoverride")) alwaysCanOverride = creationArguments["alwayscanoverride"] == "1";
            if (creationArguments.ContainsKey("prints")) previousPrintsSerialized = creationArguments["prints"];

            if (string.IsNullOrEmpty(langParameter))
            {
                langParameter = "en"; //English is the default language when no parameter passed.
                if (! string.IsNullOrEmpty(ConfigurationManager.AppSettings["lang"])) // if previously saved in config, load from config
                {
                    langParameter = ConfigurationManager.AppSettings["lang"];
                }
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
            if (!string.IsNullOrEmpty(previousPrintsSerialized))
            {
                result.LoadPreviousPrints(previousPrintsSerialized);
            }
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

        internal List<ImportedPrint> ImportedPrints {get; private set; }

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

        public void LoadPreviousPrints(string serializedData)
        {
            CapturedPrintData capturedData = ObjectSerializer.GetInstanceFromString<CapturedPrintData>(serializedData);
            List<FingerprintData> fingers = capturedData.Prints;
            this.ImportedPrints = new List<ImportedPrint>();
            
            foreach (var finger in fingers)
            {
                Bitmap bmpInstance = null;
                if (finger.ImageDataFormat == PrintDataFormat.Wsq)
                {
                    throw new ApplicationException("Cannot load Wsq prints for new Print Capture!");
                }
                var bmpRawData = finger.ImageDataFormat == PrintDataFormat.BmpZip ?
                            SevenZipHelper.Decompress(finger.ImageData) : finger.ImageData;
                if (bmpRawData != null)
                {
                    var printSize = new Size(finger.ImageInfo.HLL, finger.ImageInfo.VLL);
                    var printRect = new Rectangle(new Point(0, 0), printSize);
                    bmpInstance = ImageUtilities.ByteArrayToBitmap(bmpRawData, printSize, printRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed, finger.ImageInfo.DPI);
                }

                var newPrint = new ImportedPrint { 
                    IsEndorsement = finger.IsEndorsement, 
                    NistPosition = finger.Position, 
                    MissingDate = finger.Missing?.Date,
                    MissingCode = finger.Missing?.NistCode,
                    OverrideCode = finger.Override == null ? "" : finger.Override.ReasonCode.ToString(),
                    OverrideReason= finger.Override?.Description,
                    Image = bmpInstance,
                    Dpi = finger.ImageInfo.DPI
                };
                this.ImportedPrints.Add(newPrint);
            }
        }
    }
}

internal class ImportedPrint
{
    public int NistPosition { get; set; }

    public bool IsEndorsement { get; set; }

    /// <summary>
    /// Null = not a missing finger
    /// </summary>
    public string MissingDate { get; set; }

    public string MissingCode { get; set; }

    public string OverrideCode { get; set; }

    public string OverrideReason { get; set; }

    public int Dpi { get; set; }

    public Bitmap Image { get; set; }
}
