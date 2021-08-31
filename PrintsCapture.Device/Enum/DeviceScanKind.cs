namespace PrintsCapture.Device.Enum
{
    using System;

    [Flags]
    public enum DeviceScanKind
    {
        FlatSingleFinger = 1,
        FlatTwoFinger = 2,
        FlatFourFinger = 4,
        RolledSingleFinger = 8,
        FlatPartialPalm = 16,
        FlatCompletePalm = 32,
        RolledPalm = 64,

        AllScanTypes = 1023
    }
}
