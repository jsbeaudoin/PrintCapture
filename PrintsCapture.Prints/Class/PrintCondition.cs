namespace PrintsCapture.Prints
{
    using PrintsCapture.Prints.Enum;

    public class PrintCondition
    {
        public PrintCondition(string code, HandPartStatus status, string text)
        {
            this.Code = code;
            this.Status = status;
            this.Text = text;
        }

        public string Code { get; private set; }

        public string Text { get; private set; }

        public HandPartStatus Status { get; private set; }
    }
}
