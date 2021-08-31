namespace PrintsCapture.Prints
{
    using System;

    using PrintsCapture.Prints.ViewModel;

    public delegate void PrintClickedEventHandler(object sender, PrintClickedEventArgs e);

    public class PrintClickedEventArgs : EventArgs
    {
        public PrintElementViewModel PrintViewModel { get; private set; }

        public PrintClickedEventArgs(PrintElementViewModel printViewModel)
        {
            this.PrintViewModel = printViewModel;
        }
    }
}
