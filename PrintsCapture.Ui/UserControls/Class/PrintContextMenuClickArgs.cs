using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintsCapture.Ui.UserControls
{
    using System.Windows;

    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.ViewModel;

    public class PrintContextMenuClickArgs : RoutedEventArgs
    {
        public PrintContextMenuClickArgs(PrintElementViewModel viewModel, PrintZoomAction action)
        {
            this.Print = viewModel;
            this.Action = action;
        }

        public PrintElementViewModel Print { get; private set; }

        public PrintZoomAction Action { get; private set; }
    }
}
