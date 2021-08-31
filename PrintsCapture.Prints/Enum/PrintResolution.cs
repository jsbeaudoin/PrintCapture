namespace PrintsCapture.Prints.Enum
{
    using System;

    [Flags]
    public enum PrintResolution
    {
        None = 0,
        Dpi500 = 1,
        Dpi1000 = 2
    }

    public static class PrintResolutionExtension
{
        public static int ToDpi(this PrintResolution resolution)
        {
            if (resolution == PrintResolution.None)
            {
                return 0;
            }

            return resolution == PrintResolution.Dpi500 ? 500 : 1000;
        }

        public static PrintResolution ToResolution(this int resolution)
        {
            if (resolution != 500 && resolution != 1000)
            {
                return PrintResolution.None;
            }

            return resolution == 500
                    ? PrintResolution.Dpi500
                    : PrintResolution.Dpi1000;
        }

        public static string ToDescription(this PrintResolution resolution)
        {
            string desc = string.Empty;
            if (resolution.IsSupported(PrintResolution.Dpi500))
            {
                desc += "500";
            }

            if (resolution.IsSupported(PrintResolution.Dpi1000))
            {
                desc += (string.IsNullOrEmpty(desc) ? "" : ", ") + "1000";
            }

            return desc;
        }

        public static bool IsSupported(this PrintResolution resolution, PrintResolution targetResolution)
        {
            return (resolution & targetResolution) == targetResolution;
        }
}
}
