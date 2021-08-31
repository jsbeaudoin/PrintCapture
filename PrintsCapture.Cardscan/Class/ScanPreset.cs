namespace PrintsCapture.Cardscan
{
    using System.Collections.Generic;

    public class ScanPreset
    {
        public int Code { get; set; }

        public string Name { get; set; }

        public string Label { get; set; }

        public float Offset { get; set; }

        public float Size { get; set; }

        public string AssociatedModel { get; set; }

        public static List<ScanPreset> GetBaseList()
        {
            var result = new List<ScanPreset>();

            result.Add(new ScanPreset { Code = 10, Name = "PresetC216", Offset = 0.065F, Size = 0.50F, AssociatedModel = "C216"});
            result.Add(new ScanPreset { Code = 11, Name = "PresetFD258", Offset = 0.125F, Size = 0.60F, AssociatedModel = "FD258" });

            result.Add(new ScanPreset { Code = 1, Name = "PresetCompletePage", Offset = 0F, Size = 1F });
            result.Add(new ScanPreset { Code = 2, Name = "PresetUpperHalf", Offset = 0F, Size = 0.5F });
            result.Add(new ScanPreset { Code = 3, Name = "PresetLowerHalf", Offset = 0.5F, Size = 0.5F });
            result.Add(new ScanPreset { Code = 4, Name = "PresetUpperTwoThird", Offset = 0F, Size = 0.66F });
            result.Add(new ScanPreset { Code = 5, Name = "PresetLowerTwoThird", Offset = 0.33F, Size = 0.66F });

            return result;
        }
    }

}
