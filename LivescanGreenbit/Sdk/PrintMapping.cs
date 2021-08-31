using System.Collections.Generic;
using System.Linq;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_DeviceCharacteristicsDefines;
using PrintsCapture.Prints.Enum;

namespace PrintsCapture.Device.LivescanGreenbit.Sdk
{
    internal class PrintMapping
    {
        public Hand PrintHand { get; set; }
        public HandPart HandPart { get; set; }
        public HandScanKind ScanKind { get; set; }

        public uint ObjectToScanId { get; set; }
    }

    internal static class PrintMappings
    {
        static List<PrintMapping> mappings = new List<PrintMapping>();

        static PrintMappings()
        {
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_INDEX, HandPart = HandPart.Index, PrintHand = Hand.Left, ScanKind = HandScanKind.Rolled});
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_MIDDLE, HandPart = HandPart.Middle, PrintHand = Hand.Left, ScanKind = HandScanKind.Rolled });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_RING, HandPart = HandPart.Ring, PrintHand = Hand.Left, ScanKind = HandScanKind.Rolled });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_LITTLE, HandPart = HandPart.Little, PrintHand = Hand.Left, ScanKind = HandScanKind.Rolled });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_THUMB, HandPart = HandPart.Thumb, PrintHand = Hand.Left, ScanKind = HandScanKind.Rolled });

            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_INDEX, HandPart = HandPart.Index, PrintHand = Hand.Right, ScanKind = HandScanKind.Rolled });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_MIDDLE, HandPart = HandPart.Middle, PrintHand = Hand.Right, ScanKind = HandScanKind.Rolled });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_RING, HandPart = HandPart.Ring, PrintHand = Hand.Right, ScanKind = HandScanKind.Rolled });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_LITTLE, HandPart = HandPart.Little, PrintHand = Hand.Right, ScanKind = HandScanKind.Rolled });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_THUMB, HandPart = HandPart.Thumb, PrintHand = Hand.Right, ScanKind = HandScanKind.Rolled });

            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_INDEX, HandPart = HandPart.Index, PrintHand = Hand.Left, ScanKind = HandScanKind.Flat });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_MIDDLE, HandPart = HandPart.Middle, PrintHand = Hand.Left, ScanKind = HandScanKind.Flat });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_RING, HandPart = HandPart.Ring, PrintHand = Hand.Left, ScanKind = HandScanKind.Flat });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_LITTLE, HandPart = HandPart.Little, PrintHand = Hand.Left, ScanKind = HandScanKind.Flat });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_THUMB, HandPart = HandPart.Thumb, PrintHand = Hand.Left, ScanKind = HandScanKind.Flat });

            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_INDEX, HandPart = HandPart.Index, PrintHand = Hand.Right, ScanKind = HandScanKind.Flat });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_MIDDLE, HandPart = HandPart.Middle, PrintHand = Hand.Right, ScanKind = HandScanKind.Flat });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_RING, HandPart = HandPart.Ring, PrintHand = Hand.Right, ScanKind = HandScanKind.Flat });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_LITTLE, HandPart = HandPart.Little, PrintHand = Hand.Right, ScanKind = HandScanKind.Flat });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_THUMB, HandPart = HandPart.Thumb, PrintHand = Hand.Right, ScanKind = HandScanKind.Flat });
            

            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS, HandPart = HandPart.TwoThumbs, PrintHand = Hand.None, ScanKind = HandScanKind.Flat });

            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT, HandPart = HandPart.FourFlats, PrintHand = Hand.Left, ScanKind = HandScanKind.Flat });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT, HandPart = HandPart.FourFlats, PrintHand = Hand.Right, ScanKind = HandScanKind.Flat });


            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_LOWER_HALF_PALM_LEFT, HandPart = HandPart.LowerPalm, PrintHand = Hand.Left, ScanKind = HandScanKind.Flat });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_LEFT, HandPart = HandPart.UpperPalm, PrintHand = Hand.Left, ScanKind = HandScanKind.Flat });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_WRITER_PALM_LEFT, HandPart = HandPart.Hypothenar, PrintHand = Hand.Left, ScanKind = HandScanKind.Flat });

            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_LOWER_HALF_PALM_RIGHT, HandPart = HandPart.LowerPalm, PrintHand = Hand.Right, ScanKind = HandScanKind.Flat });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_UPPER_HALF_PALM_RIGHT, HandPart = HandPart.UpperPalm, PrintHand = Hand.Right, ScanKind = HandScanKind.Flat });
            mappings.Add(new PrintMapping { ObjectToScanId = GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_WRITER_PALM_RIGHT, HandPart = HandPart.Hypothenar, PrintHand = Hand.Right, ScanKind = HandScanKind.Flat });
            
        }

        internal static PrintMapping GetMapping(uint id)
        {
            return mappings.FirstOrDefault(x => x.ObjectToScanId == id);
        }

        internal static PrintMapping GetMapping(Hand printHand, HandPart handPart, HandScanKind scanKind)
        {
            return
                mappings.FirstOrDefault(
                    x => x.HandPart == handPart && x.PrintHand == printHand && x.ScanKind == scanKind);
        }

    }
}
