namespace PrintsCapture.Prints.Enum
{
    using System;

    [Flags]
    public enum PrintCaptureGroup
    {
        Unknown = 0,
        Standard14 = 1,
        FlatOnly = 2,
        StandardAndPalm = 4,
        OneFingerOnly = 8
    }
}
