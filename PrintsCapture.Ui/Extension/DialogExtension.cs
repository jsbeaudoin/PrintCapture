namespace PrintsCapture.Ui.Extension
{
    public static class DialogExtension
    {
        public static bool IsAccepted(this bool? dialogResult)
        {
            return (dialogResult.HasValue && dialogResult.Value);
        }
    }
}
