using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintsCapture.Ui.ViewModel
{
    using System.Windows.Media;

    public class PalmQualityControlViewModel
    {

        public ImageSource PalmImage { get; set; }

        public string Message { get; set; }

        public bool? Accepted { get; set; }

    }
}
