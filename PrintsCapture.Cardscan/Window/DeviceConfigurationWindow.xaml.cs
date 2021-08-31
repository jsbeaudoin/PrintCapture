using System.Collections.Generic;
using System.Linq;
using PrintsCapture.Cardscan.Class;

namespace PrintsCapture.Cardscan.Window
{
    using System.Windows;

    using PrintsCapture.Cardscan.ViewModel;
    using PrintsCapture.Prints.Language;

    /// <summary>
    /// Interaction logic for DeviceConfigurationWindow.xaml
    /// </summary>
    public partial class DeviceConfigurationWindow : Window
    {
        public DeviceConfigurationWindow(CardScanConfigurationViewModel viewModel)
        {
            this.ViewModel = viewModel;
            InitializeComponent();

            
        }

        public CardScanConfigurationViewModel ViewModel { get; private set; }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.ViewModel.SelectedDevice.SerialNumber))
            {
                MessageBox.Show(CommonText.SerialNumberNeeded);
                return;
            }
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void AutoDetectScannerButtonClick(object sender, RoutedEventArgs e)
        {
            var list = TestUsbDetect.GetUSBDevices().Select(x => x.Description.ToLowerInvariant()).ToList();

            var existing =  this.ViewModel.DeviceList.FirstOrDefault(x => this.AnyContains(list, x.DetectKey));

            if (existing != null)
            {
                this.ViewModel.SelectedDevice = existing;
            }                    

        }

        private bool AnyContains(IEnumerable<string> list, string detectKey)
        {
            if (string.IsNullOrEmpty(detectKey) || list == null )
            {
                return false;
            }

            foreach (var text in list)
            {
                if (text.Contains(detectKey))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
