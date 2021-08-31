namespace PrintsCapture.Livescan.ViewModel
{
    using System.Windows.Media.Imaging;

    using PrintsCapture.Prints.Enum;

    public class ConditionElementViewModel
    {
        public BitmapImage Image { get; set; }

        public string Label { get; set; }

        public HandPartStatus Status { get; set; }

        public bool IsSelected { get; set; }
    }
}
