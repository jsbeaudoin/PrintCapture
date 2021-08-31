namespace PrintsCapture.Ui.UserControls
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.ViewModel;
    using PrintsCapture.Ui.ViewModel;

    /// <summary>
    /// Interaction logic for PrintGridControl.xaml
    /// </summary>
    public partial class PrintGridControl : INotifyPropertyChanged
    {
        public PrintGridControl()
        {            
            InitializeComponent();
            
        }        

        public PrintGridViewModel PrintList
        {
            get
            {
                return (PrintGridViewModel)this.GetValue(PrintListProperty);
            }
            set
            {
                if (this.PrintList == value)
                {
                    return;
                }
                this.SetValue(PrintListProperty, value);
                this.OnPropertyChanged("PrintList");
            }
        }
        

        public delegate void ViewModelClickHandler(Object sender, PrintClickEventArgs e);

        public delegate void ViewModelContextClickHandler(Object sender, PrintContextMenuClickArgs e);

        public event ViewModelClickHandler PrintImageClick;

        public event ViewModelContextClickHandler PrintContextMenuClick;

        public static readonly DependencyProperty PrintListProperty =
        DependencyProperty.Register("PrintList", typeof(PrintGridViewModel), typeof(PrintGridControl), new UIPropertyMetadata(null));
        
        private void PrintElementMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var ctrl = sender as FrameworkElement;
            if (ctrl == null || ctrl.Tag == null || e.ChangedButton != MouseButton.Left)
            {
                return;
            }            

            var vm = (PrintElementViewModel)ctrl.Tag;                        
            this.OnPrintImageClick(sender, vm);
        }                        

        public void OnPrintImageClick(Object sender, PrintElementViewModel viewModel)
        {
            var handler = this.PrintImageClick;
            if (handler == null)
            {
                return;
            }

            handler(sender, new PrintClickEventArgs(viewModel));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ScanAgainContextMenuClick(object sender, RoutedEventArgs e)
        {
            this.TriggerMenuClick(sender, PrintZoomAction.Scan);
        }

        private void EditPrintContextMenuClick(object sender, RoutedEventArgs e)
        {
            this.TriggerMenuClick(sender, PrintZoomAction.Edit);
        }

        private void TriggerMenuClick(object sender, PrintZoomAction action)
        {
            var cm = ((MenuItem)sender).Parent as ContextMenu;
            var source = cm.PlacementTarget as Border;

            if (source == null)
            {
                return;
            }

            var vm = source.Tag as PrintElementViewModel;

            var handler = this.PrintContextMenuClick;
            if (handler == null)
            {
                return;
            }

            handler(sender, new PrintContextMenuClickArgs(vm, action));
        }
    }
    
}
