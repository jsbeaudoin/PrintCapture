namespace PrintsCapture.Prints
{
    public class PrintModifiedEventArgs
    {
        public PrintInfo Info { get; private set; }

        public PrintModifiedEventArgs(PrintInfo info)
        {
            this.Info = info;
        }
    }
}
