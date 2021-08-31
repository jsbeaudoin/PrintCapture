using System.Collections.Generic;
using System.Linq;

namespace PrintsCapture.Device.LivescanCrossmatchEssential.Plugin
{
    using System;
    using System.Diagnostics;

    using Livescan.Scanners.DriverEssential.Sdk;

    using PrintsCapture.Device.Enum;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;

    public class LedManager
    {
        private readonly int deviceHandle;

        private readonly IEnumerable<PhysicalHandPart> handParts;

        public bool SupportLeds { get; private set; }
        private readonly List<LedController> leds = new List<LedController>();
        private readonly Dictionary<int, int> nistIndexToLedIndex = new Dictionary<int, int>();

        private bool guardianStatusLed;

        private uint ledForCaptureType = 0;

        public LedManager(int deviceHandle, IEnumerable<PhysicalHandPart> handParts)
        {
            enumLScanLedType ledType;
            uint availableLeds;
            int ledCount;
            this.deviceHandle = deviceHandle;
            this.handParts = handParts;
            LSE_SDK.LSCAN_Controls_GetAvailableLEDs(this.deviceHandle, out ledType, out ledCount, out availableLeds);

            LScanDeviceInfo devInfo;
            LSE_SDK.LSCAN_Main_GetDeviceInfo(this.deviceHandle, out devInfo);

            this.guardianStatusLed = devInfo.typeName.ToLowerInvariant().Contains("guardian");            

            this.InitLeds();
            this.SupportLeds = ledType == enumLScanLedType.LSCAN_LED_TYPE_STATUS;
        }

        public void CloseLeds()
        {
            if (!this.SupportLeds) return;
            LSE_SDK.LSCAN_Controls_SetActiveLEDs(this.deviceHandle, 0);
        }

        public void InitForCapture(HandPart part, Hand hand)
        {
            if (!this.SupportLeds) return;

            nistIndexToLedIndex.Clear();
            uint value = 0;
            
            const PrintQualityLevel PresenceQuality = PrintQualityLevel.Unknown;

            this.DefineNistIndexesMatch(part, hand);            
            this.SetLedForCaptureType(part, hand);

            foreach (var ledIndex in nistIndexToLedIndex.Values)
            {
                int index = ledIndex;
                value += leds.Single(x => x.Index == index).GetValueForQuality(PresenceQuality);
            }

            LSE_SDK.LSCAN_Controls_SetActiveLEDs(this.deviceHandle, value + this.ledForCaptureType);

        }

        private void SetLedForCaptureType(HandPart part, Hand hand)
        {
            uint value = 0;
            var leftHandLed = LSE_LED_Constants.LSCAN_LED_I1_GREEN_B1 | LSE_LED_Constants.LSCAN_LED_I1_GREEN_B2;
            var rightHandLed = this.guardianStatusLed
                ? LSE_LED_Constants.LSCAN_LED_I3_GREEN_B1 | LSE_LED_Constants.LSCAN_LED_I3_GREEN_B2
                : LSE_LED_Constants.LSCAN_LED_I4_GREEN_B1 | LSE_LED_Constants.LSCAN_LED_I4_GREEN_B2;
            var leftThumbLed = LSE_LED_Constants.LSCAN_LED_I2_GREEN_B1 | LSE_LED_Constants.LSCAN_LED_I2_GREEN_B2;
            var rightThumbLed = this.guardianStatusLed
                ? LSE_LED_Constants.LSCAN_LED_I4_GREEN_B1 | LSE_LED_Constants.LSCAN_LED_I4_GREEN_B2
                : LSE_LED_Constants.LSCAN_LED_I3_GREEN_B1 | LSE_LED_Constants.LSCAN_LED_I3_GREEN_B2;


            if (part == HandPart.FourFlats && hand == Hand.Left)
            {
                value = leftHandLed;
            }
            else if (part == HandPart.FourFlats && hand == Hand.Right)
            {
                value = rightHandLed;
            }
            else if (part == HandPart.Thumb && hand == Hand.Left)
            {
                value = leftThumbLed;
            }
            else if (part == HandPart.Thumb && hand == Hand.Right)
            {
                value = rightThumbLed;
            }
            else if (part == HandPart.TwoThumbs)
            {
                value = leftThumbLed | rightThumbLed;
            }
            
            this.ledForCaptureType = value;
        }

        private void DefineNistIndexesMatch(HandPart part, Hand hand)
        {
            var isleft = hand == Hand.Left;

            switch (part)
            {
                case HandPart.FourFlats:
                    this.AddFinger(hand, HandPart.Index, isleft ? 4 : 1);
                    this.AddFinger(hand, HandPart.Middle, isleft ? 3 : 2);
                    this.AddFinger(hand, HandPart.Ring, isleft ? 2 : 3);
                    this.AddFinger(hand, HandPart.Little, isleft ? 1 : 4);
                    
                    break;

                case HandPart.TwoThumbs:
                    this.AddFinger(Hand.Left, HandPart.Thumb, 2);
                    this.AddFinger(Hand.Right, HandPart.Thumb, 3);                    

                    break;

                case HandPart.CompletePalm:
                    case HandPart.Hypothenar:
                    case HandPart.LowerPalm:
                    case HandPart.UpperPalm:

                    break;

                case HandPart.Index:
                    this.AddFinger(hand, HandPart.Index, isleft ? 4 : 1);
                    break;

                case HandPart.Middle:
                    this.AddFinger(hand, HandPart.Middle, isleft ? 3 : 2);
                    break;

                case HandPart.Ring:
                    this.AddFinger(hand, HandPart.Ring, isleft ? 2 : 3);
                    break;

                case HandPart.Little:
                    this.AddFinger(hand, HandPart.Little, isleft ? 1 : 4);
                    break;  

                case HandPart.Thumb:
                    this.AddFinger(hand, HandPart.Thumb, isleft ? 2 : 3);
                    break;
                
                default:
                    this.nistIndexToLedIndex.Add(0, isleft ? 2 : 3);

                    break;
            }
        }

        

        public void DisplayQuality(List<PrintCaptureQuality> printQualities)
        {
            if (!this.SupportLeds) return;

            uint value = 0;

            foreach (var captureQuality in printQualities)
            {
                
                var index = captureQuality.HandPart.EndorsementIndex;
                if (this.nistIndexToLedIndex.ContainsKey(index))
                {
                    var ledIndex = this.nistIndexToLedIndex[index];
                    var led = leds.SingleOrDefault(x => x.Index == ledIndex);
                    value += led.GetValueForQuality(captureQuality.Quality);
                    Console.WriteLine("Quality for {0}. Nist Index : {1}. Led index {2}", captureQuality.HandPart.HandPart, captureQuality.HandPart.EndorsementIndex, ledIndex);
                }
                else if (captureQuality.HandPart.Kind == HandPartKind.Finger &&  this.nistIndexToLedIndex.ContainsKey(0))
                {                    
                    var led = leds.SingleOrDefault(x => x.Index == 0);
                    value += led.GetValueForQuality(captureQuality.Quality);
                    Console.WriteLine("Quality for {0}. Nist Index : {1}. ** GENERIC **",captureQuality.HandPart.HandPart, captureQuality.HandPart.EndorsementIndex);
                }
                else
                {
                    Debug.Print(@"Led Manager : Undefined index {0}", index);
                }
            }            

            LSE_SDK.LSCAN_Controls_SetActiveLEDs(this.deviceHandle, value+ this.ledForCaptureType);
        }

        private void AddFinger(Hand hand, HandPart part, int ledIndex)
        {
            var phys = this.handParts.SingleOrDefault(x => x.Hand == hand && x.HandPart == part && x.Kind == HandPartKind.Finger);

            if (phys.IsMissing)
            {
                return;
            }

            this.nistIndexToLedIndex.Add(phys.EndorsementIndex, ledIndex);
        }

        private void InitLeds()
        {
            leds.Add(new LedController(1, LSE_LED_Constants.LSCAN_LED_S1_GREEN_B1, LSE_LED_Constants.LSCAN_LED_S1_GREEN_B2, LSE_LED_Constants.LSCAN_LED_S1_RED_B1, LSE_LED_Constants.LSCAN_LED_S1_RED_B2));
            leds.Add(new LedController(2, LSE_LED_Constants.LSCAN_LED_S2_GREEN_B1, LSE_LED_Constants.LSCAN_LED_S2_GREEN_B2, LSE_LED_Constants.LSCAN_LED_S2_RED_B1, LSE_LED_Constants.LSCAN_LED_S2_RED_B2));
            leds.Add(new LedController(3, LSE_LED_Constants.LSCAN_LED_S3_GREEN_B1, LSE_LED_Constants.LSCAN_LED_S3_GREEN_B2, LSE_LED_Constants.LSCAN_LED_S3_RED_B1, LSE_LED_Constants.LSCAN_LED_S3_RED_B2));
            leds.Add(new LedController(4, LSE_LED_Constants.LSCAN_LED_S4_GREEN_B1, LSE_LED_Constants.LSCAN_LED_S4_GREEN_B2, LSE_LED_Constants.LSCAN_LED_S4_RED_B1, LSE_LED_Constants.LSCAN_LED_S4_RED_B2));
        }

        private class LedController
        {
            private readonly uint greenBlink1;

            private readonly uint greenBlink2;

            private readonly uint redBlink1;

            private readonly uint redBlink2;

            public LedController(int index, uint greenBlink1, uint greenBlink2, uint redBlink1, uint redBlink2)
            {
                this.Index = index;
                this.greenBlink1 = greenBlink1;
                this.greenBlink2 = greenBlink2;
                this.redBlink1 = redBlink1;
                this.redBlink2 = redBlink2;
            }

            public int Index { get; private set; }

            public uint GetValueForQuality(PrintQualityLevel quality)
            {
                switch (quality)
                {
                        case PrintQualityLevel.Unknown:
                            return this.greenBlink1; // blinks green
                        
                        case PrintQualityLevel.Good:
                            return this.greenBlink1 | this.greenBlink2; // green not blinking
                        
                        case PrintQualityLevel.Bad:
                            return this.redBlink1 | this.redBlink2; // red non blinking
                        
                        case PrintQualityLevel.NotGoodEnough:
                            return this.greenBlink2; // green blink fast
                        
                        case PrintQualityLevel.NotPresent:
                            return this.greenBlink1;
                        
                }

                return 0;
            }
            
        }

    }
}
