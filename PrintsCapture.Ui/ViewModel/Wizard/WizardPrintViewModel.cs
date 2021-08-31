using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PrintsCapture.Prints;
using PrintsCapture.Prints.ViewModel;

namespace PrintsCapture.Ui.ViewModel.Wizard
{

    public enum PrintDisplayStyle
    {
        Ok,
        Modified,
        ToScan
        
    }

    public class WizardPrintViewModel : INotifyPropertyChanged
    {
        private static Dictionary<PrintDisplayStyle, Style> styleList;
        private Style displayStyle;
        private PrintElementViewModel print;

        static WizardPrintViewModel()
        {
            styleList = new Dictionary<PrintDisplayStyle, Style>();

            CreateStyle(PrintDisplayStyle.Ok, Color.FromArgb(100,0,255,0));
            CreateStyle(PrintDisplayStyle.Modified, Color.FromArgb(100, 173, 255, 47));
            CreateStyle(PrintDisplayStyle.ToScan, Color.FromArgb(100, 0, 0, 255));
        }


        private static void CreateStyle(PrintDisplayStyle styleKind, Color color)
        {
            var style = new Style(typeof(Border));
            var setter = new Setter(Border.BackgroundProperty, new SolidColorBrush(color));


            style.Setters.Add(setter);
            styleList.Add(styleKind, style);
        }

        public PrintElementViewModel Print
        {
            get { return print; }
            set
            {
                if (print == value)
                {
                    return;
                }
                print = value; 
                this.OnPropertyChanged(nameof(this.Print));
            }
        }

        public WizardPrintViewModel(PrintElementViewModel print)
        {
            this.Update(print);
        }

        public void Update(PrintElementViewModel print)
        {
            Print = print;
            this.SetStyle();
        }

        public Style DisplayStyle
        {
            get { return displayStyle; }
            set
            {
                if (this.displayStyle == value)
                {
                    return;
                }
                displayStyle = value;
                this.OnPropertyChanged(nameof(this.DisplayStyle));
            }
        }

        public void SetStyle()
        {
            var action = this.Print.LinkedPrint.UserAction;

            if ((action & WizardAction.OverrideCodeMask) > 0)
            {
                this.DisplayStyle = styleList[PrintDisplayStyle.Modified];
                return;
            }


            if (action == WizardAction.Accept)
            {
                this.DisplayStyle = styleList[PrintDisplayStyle.Ok];
            }
            else
            {
                this.DisplayStyle = styleList[PrintDisplayStyle.ToScan];
            }
            

            

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
