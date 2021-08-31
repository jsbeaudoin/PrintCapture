namespace PrintsCapture.LocalNeuroSeqCheck
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using Neurotec.Biometrics;
    using Neurotec.Biometrics.Tools;
    using Neurotec.Images;
    using Neurotec.IO;

    using NLog;

    public class NeuroHelper
    {
        private const int MinutesBetweenCheck = 20;

        private NMatcher neuroMatcher;

        static readonly object MatcherSync = new object();

        private readonly Logger logger;

        private NMatcher Matcher
        {
            get
            {
                return this.neuroMatcher ?? (this.neuroMatcher = new NMatcher());
            }
        }

        private bool IsInitialized { get; set; }

        private DateTime lastCheck;

        private int licencePort;

        public NeuroHelper(int port)
        {
            this.logger = LogManager.GetCurrentClassLogger();

            this.licencePort = port;

            this.CheckNeuroLicense();
        }

        public int MatchOneToOne(byte[] mainFinger, byte[] compareFinger)
        {
            this.CheckNeuroLicense();

            try
            {
                lock (MatcherSync)
                {
                    return this.Matcher.Verify(mainFinger, compareFinger);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Error with Neuro Verify");
                throw;
            }
        }

        public List<SequenceCheckResult> AntiSequencing(FingerTemplate mainFinger, List<FingerprintTemplates> fingerprints)
        {
            this.CheckNeuroLicense();

            var result = new List<SequenceCheckResult>();

            foreach (var compareFinger in fingerprints)
            {
                if (mainFinger.Position != compareFinger.Position)
                {
                    this.Matcher.IdentifyStart(mainFinger.Template);

                    foreach (var compareTemplate in compareFinger.Templates)
                    {
                        var score = this.Matcher.IdentifyNext(compareTemplate.Template);

                        result.Add(new SequenceCheckResult
                        {
                            Position = compareFinger.Position,
                            Score = score
                        });
                    }

                    this.Matcher.IdentifyEnd();
                }
            }

            return result;
        }

        public List<SequenceCheckResult> SequenceCheck(FingerTemplate finger, List<FingerprintTemplates> fingerprints)
        {
            if (finger.Template == null)
            {
                return null;
            }

            this.CheckNeuroLicense();

            this.Matcher.IdentifyStart(finger.Template);

            var result = new List<SequenceCheckResult>();

            foreach (var template in fingerprints)
            {
                foreach (var fingerTemplate in template.Templates)
                {
                    var score = this.Matcher.IdentifyNext(fingerTemplate.Template);

                    result.Add(new SequenceCheckResult
                    {
                        Position = fingerTemplate.Position,
                        Score = score
                    });
                }
            }

            this.Matcher.IdentifyEnd();

            return result;
        }

        public List<SequenceCheckResult> SequenceCheck(byte[] finger, Dictionary<NFPosition, byte[]> fingerprints)
        {
            if (finger == null)
            {
                return null;
            }

            this.CheckNeuroLicense();

            this.Matcher.IdentifyStart(finger);

            var result = new List<SequenceCheckResult>();

            foreach (var template in fingerprints)
            {                
                    var score = this.Matcher.IdentifyNext(template.Value);

                    result.Add(new SequenceCheckResult
                    {
                        Position = template.Key,
                        Score = score
                    });                
            }

            this.Matcher.IdentifyEnd();

            return result;
        }        

        public List<FingerTemplateDto> GetTemplatesDto(Bitmap image, int position, List<int> missingFinger)
        {
            var templates = this.GetTemplates(image, position, missingFinger);

            return templates.Select(fingerTemplate => new FingerTemplateDto
            {
                Position = (int)fingerTemplate.Position,
                Template = fingerTemplate.Template,
                InError = fingerTemplate.Status != NfeExtractionStatus.TemplateCreated
            }).ToList();
        }

        public NeuroPrintInfo AnalyzePrint(Bitmap image, int position, List<int> missingFinger)
        {
            this.CheckNeuroLicense();

            var result = new NeuroPrintInfo();
            var step = "NImage creation";

            try
            {
                var ni = NImage.FromBitmap(image);
                step = "Gray scale creation";

                var gray = this.ConvertGrayScalImage(ni);

                step = "Quality computation";
                result.Quality = StopwatchLogger.Run(() => (int)Nfiq.Compute(gray), "Nfiq.Compute");

                step = "Template Creation";
                var templates = new List<FingerTemplate>();

                var slapPosition = (NFPosition)position;

                if (position >= 11 && position <= 15)
                {
                    var missingPositions = this.GetHandMissingFingers(slapPosition, missingFinger.Cast<NFPosition>());

                    var segments = StopwatchLogger.Run(() => NFSegmenter.Locate(gray, slapPosition, missingPositions), "NFSegmenter.Locate");
                    var items = StopwatchLogger.Run(() => NFSegmenter.CutMultiple(gray, segments), "NFSegmenter.CutMultiple");

                    for (var i = 0; i < items.Length; i++)
                    {
                        if (segments[i].Status == NBiometricStatus.Ok)
                        {
                            var t = this.ExtractFingerTemplate(segments[i].Position, items[i], NFImpressionType.LiveScanPlain);

                            StopwatchLogger.Run(() =>
                            {
                                t.RectTop = Math.Min(segments[i].TopLeft.Y, segments[i].TopRight.Y);
                                t.RectLeft = Math.Min(segments[i].TopLeft.X, segments[i].BottomLeft.X);
                                var bottom = Math.Max(segments[i].BottomLeft.Y, segments[i].BottomRight.Y);
                                var right = Math.Max(segments[i].TopRight.X, segments[i].BottomRight.X);
                                t.RectHeight = bottom - t.RectTop + 1;
                                t.RectWidth = right - t.RectLeft + 1;
                                t.Score = (int)Nfiq.Compute(items[i]);
                            }, "Nfiq.Compute (Segment)");

                            templates.Add(t);

                            this.logger.Debug("Template stored. Original Position {0}, template position {1}. Segment score:{2}", position, segments[i].Position, t.Score);
                        }

                        items[i].Dispose();
                    }
                }
                else
                {
                    templates.Add(this.ExtractFingerTemplate(slapPosition, gray, NFImpressionType.LiveScanRolled));
                }

                result.Templates = templates;
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "NeuroTools.AnalyzePrint error at step : {0}", step);

                throw;
            }

            return result;
        }

        public List<FingerTemplate> GetTemplates(Bitmap image, int position, List<int> missingFinger)
        {
            this.CheckNeuroLicense();

            return StopwatchLogger.Run(() =>
            {

                var originalImage = NImage.FromBitmap(image);
                var grayscaleImage = this.ConvertGrayScalImage(originalImage);

                var templates = new List<FingerTemplate>();
                var slapPosition = (NFPosition)position;

                if ((position == 13) || (position == 14) || (position == 11) || (position == 12))
                {
                    var missingPositions = this.GetHandMissingFingers(slapPosition, missingFinger.Cast<NFPosition>());
                    var segments = NFSegmenter.Locate(grayscaleImage, slapPosition, missingPositions);
                    var items = NFSegmenter.CutMultiple(grayscaleImage, segments);

                    for (var i = 0; i < items.Length; i++)
                    {
                        if (segments[i].Status == NBiometricStatus.Ok)
                        {
                            this.logger.Debug("Template stored. Original Position {0}, template position {1}", position, segments[i].Position);

                            var template = this.ExtractFingerTemplate(segments[i].Position, items[i], NFImpressionType.LiveScanPlain);

                            templates.Add(template);
                        }

                        items[i].Dispose();
                    }
                }
                //   else if ((position == 11) || (position == 12))
                //  {
                //      templates.Add(ExtractFingerTemplate((NFPosition)position, grayscaleImage, NFImpressionType.LiveScanPlain));
                //  }
                else if (position < 13 || position == 99)
                {
                    var template = this.ExtractFingerTemplate(slapPosition, grayscaleImage, NFImpressionType.LiveScanRolled);
                    templates.Add(template);
                }
                return templates;

            }, "GetTemplates");
        }

        public byte[] GetTemplateFromBmp(Bitmap image, int position)
        {
            this.CheckNeuroLicense();

            var originalImage = NImage.FromBitmap(image);
            var grayscaleImage = this.ConvertGrayScalImage(originalImage);

            //return ExtractTemplate(
            //    grayscaleImage,
            //    position < 11 ? NFImpressionType.LiveScanRolled : NFImpressionType.LiveScanPlain);
            return this.ExtractTemplate(grayscaleImage, NFImpressionType.LiveScanPlain, (NFPosition)position);
        }

        public ImageInfo GetImageInfo(Bitmap bmp)
        {
            this.CheckNeuroLicense();

            if (bmp == null)
            {
                throw new ApplicationException("Can't retrive information from a null image");
            }

            var image = NImage.FromBitmap(bmp);

            var result = new ImageInfo { HLL = (int)image.HorzResolution, VLL = (int)image.VertResolution };
            image.Dispose();

            return result;
        }

        public byte[] ConvertToWSQ(Bitmap bmp, float compression)
        {
            this.CheckNeuroLicense();

            if (bmp == null)
            {
                throw new ApplicationException("Can't convert a null image");
            }

            var image = this.ConvertGrayScalImage(NImage.FromBitmap(bmp));

            var wsqInfo = (WsqInfo)NImageFormat.Wsq.CreateInfo(image);

            wsqInfo.BitRate = compression;

            return image.Save(wsqInfo).ToArray();
        }

        public Bitmap ConvertToBitmap(byte[] printWsq)
        {
            this.CheckNeuroLicense();

            if (printWsq == null || printWsq.Length == 0)
            {
                return null;
            }

            var img = NImage.FromMemory(printWsq, NImageFormat.Wsq);

            return img.ToBitmap();
        }

        public int GetFingerprintQuality(Bitmap image)
        {
            this.CheckNeuroLicense();

            return StopwatchLogger.Run(() =>
            {
                var originalImage = NImage.FromBitmap(image);
                var grayscaleImage = this.ConvertGrayScalImage(originalImage);

                var quality = Nfiq.Compute(grayscaleImage);

                return (int)(quality);

            }, "GetFingerprintQuality");
        }

        public bool ActivateLicence()
        {
            bool result;
            try
            {
                result = this.CheckNeuroLicense();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Activate Licence fail");
                result = false;
            }

            return result;
        }

        #region "Private Functions"

        private bool CheckNeuroLicense()
        {
            if (this.IsInitialized)
            {
                var timeElapsed = DateTime.Now.Subtract(this.lastCheck).TotalMinutes > MinutesBetweenCheck;

                if (timeElapsed)
                {
                    this.lastCheck = DateTime.Now;

                    return NeuroLicence.CheckLicensing();
                }
            }
            else
            {
                this.IsInitialized = NeuroLicence.ActivateLicenses(this.licencePort, false);

                this.lastCheck = DateTime.Now;

                return this.IsInitialized;
            }

            return false;
        }       

        private NFPosition[] GetHandMissingFingers(NFPosition position, IEnumerable<NFPosition> missingFingers)
        {
            NFPosition[] positions;

            if (missingFingers == null)
            {
                throw new ArgumentNullException("missingFingers");
            }

            switch (position)
            {
                case NFPosition.PlainRightFourFingers:
                    positions = new[] { NFPosition.RightIndex, NFPosition.RightMiddle, NFPosition.RightRing, NFPosition.RightLittle };
                    break;

                case NFPosition.PlainLeftFourFingers:
                    positions = new[] { NFPosition.LeftIndex, NFPosition.LeftMiddle, NFPosition.LeftRing, NFPosition.LeftLittle };
                    break;

                default:
                    positions = new NFPosition[] { };
                    break;
            }

            return missingFingers.Intersect(positions).ToArray();
        }

        private byte[] ExtractTemplate(NGrayscaleImage image, NFImpressionType type, NFPosition position = NFPosition.Unknown)
        {
            using (var templateMaker = new NFExtractor(false) { UseQuality = false })
            {
                NfeExtractionStatus extractionStatus;

                var record = templateMaker.Extract(image, position, type, out extractionStatus);

                if (extractionStatus != NfeExtractionStatus.TemplateCreated)
                {
                    return null;
                }

                return record.Save().ToArray();
            }
        }

        private FingerTemplate ExtractFingerTemplate(NFPosition position, NGrayscaleImage image, NFImpressionType type)
        {
            return StopwatchLogger.Run(() =>
            {
                NFRecord record;
                NfeExtractionStatus extractionStatus;

                using (var templateMaker = new NFExtractor(false) { UseQuality = false })
                {
                    record = templateMaker.Extract(image, NFPosition.Unknown, type, out extractionStatus);
                }

                var template = new FingerTemplate
                {
                    Status = extractionStatus,
                    Position = position
                };

                if (extractionStatus == NfeExtractionStatus.TemplateCreated)
                {
                    template.Template = record.Save().ToArray();
                }

                return template;

            }, "ExtractFingerTemplate", position.ToString("F"));
        }

        private NGrayscaleImage ConvertGrayScalImage(NImage img)
        {
            NGrayscaleImage result;

            if (img.PixelFormat != NPixelFormat.Grayscale8U)
            {
                result = img.ToGrayscale();
            }
            else
            {
                var gray = (NGrayscaleImage)NImage.FromImage(NPixelFormat.Grayscale8U, img.Stride, img);
                if (!gray.ResolutionIsAspectRatio)
                {
                    if (img.HorzResolution < 250 || img.VertResolution < 250)
                    {
                        var horz = (img.HorzResolution < 250) ? 500 : img.HorzResolution;
                        var vert = (img.VertResolution < 250) ? 500 : img.VertResolution;
                        gray.VertResolution = vert;
                        gray.HorzResolution = horz;
                    }
                }
                result = gray;
            }

            return result;
        }

        #endregion
    }

    public class SequenceCheckResult
    {
        public NFPosition Position { get; set; }

        public NFPosition SubPosition { get; set; }

        public int Score { get; set; }
    }

    public class FingerTemplate
    {
        public int Index { get; set; }

        public NFPosition Position;

        public NfeExtractionStatus Status { get; set; }

        public byte[] Template;

        public int Score;

        public int RectTop { get; set; }

        public int RectLeft { get; set; }

        public int RectWidth { get; set; }

        public int RectHeight { get; set; }
    }

    public class FingerTemplateDto
    {
        public int Position;

        public byte[] Template;

        public bool InError;
    }

    public class NeuroPrintInfo
    {
        public List<FingerTemplate> Templates { get; set; }

        public int Quality { get; set; }
    }

    public class FingerprintTemplates
    {
        public int Index { get; set; }

        public NFPosition Position;

        public List<FingerTemplate> Templates;

        public int Quality;
    }

    public class ImageInfo
    {
        public int VLL { get; set; }

        public int HLL { get; set; }
    }
}