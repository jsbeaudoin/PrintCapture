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

    public static class CapturedPrintDataBuilder
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="printList">List of all prints</param>
        /// <param name="captureDevice">Capture Device Information</param>
        /// <param name="captureMode">Capture mode for prints</param>
        /// <returns></returns>
        public static CapturedPrintData GetCapturedPrintData(List<PrintInfo> printList, CaptureDeviceInfo captureDevice, string captureMode)
        {
            try
            {
            
                var impressionType = captureDevice.Kind == CaptureKind.Livescan
                                                  ? CaptureType.LiveScan
                                                  : CaptureType.CardScan;

                var data = new CapturedPrintData
                {
                    CaptureFlatOnly = captureMode == "flat",
                    CapturedTime = DateTime.Now,
                    Device =
                            new DeviceInformation()
                            {
                                Kind =
                                    captureDevice.Kind
                                    == CaptureKind.Livescan
                                        ? DeviceKind.LiveScan
                                        : DeviceKind.CardScan,
                                Manufacturer =
                                    ToRcmpText(
                                        captureDevice.Make),
                                ModelName =
                                    ToRcmpText(
                                        captureDevice.ModelName),
                                SerialNumber =
                                    ToRcmpText(
                                        captureDevice
                                    .SerialNumber)
                            },
                    Prints = new List<FingerprintData>()
                };

                foreach (var printInfo in printList)
                {
                    var p = new FingerprintData
                    {
                        IsEndorsement = printInfo.IsEndorsement,
                        Position = printInfo.NistPosition,
                        Quality = printInfo.QualityScore,
                        Sequence = printInfo.SequenceScore,
                        MinutiaCount = printInfo.MinutiaCount,
                    };

                    if (printInfo.IsEndorsement)
                    {
                        p.Position = printInfo.EndorsementFinger.EndorsementIndex;
                    }

                    if (!printInfo.IsMissing && printInfo.ImageForProcessing != null)
                    {
                        p.ImageData = printInfo.ImageForProcessing.ToByteArray();
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
            catch (Exception)
            {

            }

            return null;
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
