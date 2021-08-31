using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PrintsCapture.Ui.View
{
    using PrintsCapture.Ui.ViewModel;

    /// <summary>
    /// Interaction logic for PalmQualityControlWindow.xaml
    /// </summary>
    public partial class PalmQualityControlWindow 
    {
        public PalmQualityControlViewModel ViewModel { get; private set; }

        public PalmQualityControlWindow(PalmQualityControlViewModel viewModel)
        {
            this.ViewModel = viewModel;
            InitializeComponent();
        }

        private void NoButtonClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Accepted = false;
            this.Close();
        }

        private void YesButtonClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Accepted = true;
            this.Close();
        }
    }
}
