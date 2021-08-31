using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrintsCapture.QuickCaptureControl
{
    using System.Globalization;
    using System.Threading;

    [ToolboxBitmap(typeof(QuickCapture), "UniDAC_civil_small.ico")]
    public partial class QuickCapture: UserControl
    {
        private Language controLanguage;

        public enum Language
        {
            French = 0,
            English = 1
        }

        #region External Properties

        [Description("Le language utilisé pour les messages et l'affichage")]
        public Language ControLanguage
        {
            get
            {
                return this.controLanguage;
            }
            set
            {
                if (value != this.controLanguage)
                {
                    this.controLanguage = value;
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(value == 0 ? "fr" : "en");
                }
            }
        }

        #endregion

        #region Public Events

        #endregion

        public QuickCapture()
        {
            this.InitializeComponent();
        }

        public void StartCapture()
        {
            
        }
    }
}
