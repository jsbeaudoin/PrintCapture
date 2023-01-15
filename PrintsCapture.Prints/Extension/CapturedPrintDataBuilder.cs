using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintsCapture.Prints.Extension
{
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Sequence;
    using System.Globalization;
    using UniBIO.Services.Communication.BiometricService;
    using UniBIO.Services.Communication.TransactionService;

    using XL_ID.Utilities.Image;
    using XL_ID.Utilities.XML;

    using NLog;
    using System.IO;

    public static class CapturedPrintDataBuilder
    {
        private static Logger Logger;

        private static Logger GetLogger()
        {
            if (Logger == null)
            {
                Logger = LogManager.GetCurrentClassLogger();
            }
            return Logger;
        }

        public static CapturedPrintData GetCapturedPrintData(BaseSeqCheckService seq)
        {
            var prints = seq.GetCapturedPrints();
            var data = GetCapturePrintData(seq, prints, true);
            seq.ResetSession();
            return data;
        }

        public static void SetSerializedPrintData(BaseSeqCheckService seq, Dictionary<string, string> dicResult)
        {
            var prints = seq.GetCapturedPrints();
            var data = GetCapturePrintData(seq, prints, false); // Complete structure, without image bytes
            dicResult.Add("captureinfo", XmlSerializer.Serialize(data));
            seq.ResetSession();

            foreach (var printInfo in prints)
            {
                if (!printInfo.IsMissing && printInfo.ImageForProcessing != null)
                {
                    byte[] imageBytes;
                    using (var ms = new MemoryStream())
                    {
                        printInfo.ImageForProcessing.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                        imageBytes = printInfo.ImageForProcessing.ToByteArray();
                    }
                    
                    printInfo.ImageForProcessing.Dispose();
                    if (printInfo.Image != null)
                    {
                        printInfo.Image = null;
                    }
                    var printData = XmlSerializer.Serialize(imageBytes);
                    dicResult.Add($"captureprint{printInfo.NistPosition}", printData); // Serialized image bytes

                    GC.Collect();
                }
            }
        }
        private static CapturedPrintData GetCapturePrintData(BaseSeqCheckService seq, List<PrintInfo> prints, bool includeImageBytes)
        {
            try
            {
                var impressionType = seq.CaptureDevice.Kind == CaptureKind.Livescan
                                                  ? CaptureType.LiveScan
                                                  : CaptureType.CardScan;

                var data = new CapturedPrintData
                {
                    CaptureFlatOnly = seq.CaptureMode == "flat",
                    CapturedTime = DateTime.Now,
                    Device =
                            new DeviceInformation()
                            {
                                Kind =
                                    seq.CaptureDevice.Kind
                                    == CaptureKind.Livescan
                                        ? DeviceKind.LiveScan
                                        : DeviceKind.CardScan,
                                Manufacturer =
                                    ToRcmpText(
                                        seq.CaptureDevice.Make),
                                ModelName =
                                    ToRcmpText(
                                        seq.CaptureDevice.ModelName),
                                SerialNumber =
                                    ToRcmpText(
                                        seq.CaptureDevice
                                    .SerialNumber)
                            },
                    Prints = new List<FingerprintData>()
                };

                foreach (var printInfo in prints)
                {
                    var p = new FingerprintData
                    {
                        IsEndorsement = printInfo.IsEndorsement,
                        Position = printInfo.NistPosition,
                        Quality = printInfo.QualityScore,
                        Sequence = printInfo.SequenceScore,
                        MinutiaCount = printInfo.MinutiaCount,
                    };
                    if (includeImageBytes)
                    {
                        p.ImageData = printInfo.ImageForProcessing.ToByteArray();
                        printInfo.ImageForProcessing.Dispose();
                    }

                    if (printInfo.IsEndorsement)
                    {
                        p.Position = printInfo.EndorsementFinger.EndorsementIndex;
                    }

                    if (!printInfo.IsMissing && printInfo.ImageForProcessing != null)
                    {
                        if (data.Dpi == 0)
                        {
                            data.Dpi = printInfo.Resolution.ToDpi();
                        }
                        
                        p.ImageInfo = new ImageInformation
                        {
                            HLL = printInfo.ImageForProcessing.Width,
                            VLL = printInfo.ImageForProcessing.Height,
                            ImpressionType = impressionType
                        };
                    }

                    data.Prints.Add(p);

                    if (printInfo.IsMissing)
                    {
                        p.Missing = new MissingPrint()
                        {
                            Position = (PrintPosition)p.Position,
                            Date = printInfo.PhysicalPart.MissingDate,
                            NistCode = printInfo.PhysicalPart.MissingCode
                        };
                    }

                    if (printInfo.IsOverriden)
                    {
                        p.Override = new FingerprintOverride()
                        {
                            Description = printInfo.OverrideUserReason,
                            Position = p.Position,
                            ReasonCode = printInfo.OverrideCode
                        };
                    }

                    if (printInfo.HasSegments)
                    {
                        p.Segments = new List<SegmentData>();
                        foreach (var segInfo in printInfo.Segments)
                        {
                            var s = new SegmentData()
                            {
                                Bottom = segInfo.Position.Bottom,
                                Left = segInfo.Position.Left,
                                MinutiaCount = segInfo.MinutiaCount,
                                MissingCode = segInfo.MissingCode,
                                MissingDate = segInfo.MissingDate,
                                OverrideCode = ToRcmpText(segInfo.OverrideCode),
                                OverrideReason = ToRcmpText(segInfo.OverrideText),
                                Position = segInfo.Part.EndorsementIndex,
                                Quality = segInfo.QualityScore,
                                Right = segInfo.Position.Right,
                                Top = segInfo.Position.Top
                            };
                            p.Segments.Add(s);
                        }
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                GetLogger().Error(ex, "GetCaptureInfo failed");
                return null;
            }
        }

        
        private static string ToRcmpText(DateTime value)
        {

            return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static string ToRcmpText(int value)
        {

            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string ToRcmpText(int? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return value.ToString();
        }

        private static string ToRcmpText(string text)
        {
            if (text == null)
            {
                return null;
            }

            return text.ToUpperInvariant();
        }

        
    }
}
