namespace PrintsCapture.Ui.Print
{
    public delegate void ScanCompletedHandler(object sender, ScanCompletedEventArgs e);

    public class ScanCompletedEventArgs
    {
        public ScanCompletedEventArgs(bool success, string resultSetId)
        {
            this.Success = success;
            this.ResultSetId = resultSetId;
            
        }

        public bool Success { get; private set; }

        public string ResultSetId { get; private set; }        

    }
}
