using System.Linq;
using PrintsCapture.Device.Interface;

namespace PrintsCapture.Livescan.View
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;

    using Device;
    using ViewModel;

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ConfigurationWindow : INotifyPropertyChanged
    {
        private LiveScanConfigurationViewModel deviceConfiguration;

        private Process externalProcess;

        public ConfigurationWindow()
        {
            this.InitializeComponent();
        }

        public ConfigurationWindow(LiveScanConfigurationViewModel deviceConfig) : this()
        {
            this.DeviceConfiguration = deviceConfig;
            //this.DataContext = this.DeviceConfiguration;
        }

        /// <summary>
        /// Gets the device configuration.
        /// </summary>
        public LiveScanConfigurationViewModel DeviceConfiguration
        {
            get
            {
                return this.deviceConfiguration;
            }
            private set
            {
                if (Equals(value, this.deviceConfiguration))
                {
                    return;
                }
                this.deviceConfiguration = value;
                this.OnPropertyChanged("DeviceConfiguration");
            }
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ScannerComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.deviceConfiguration.SelectedDevice != null)
            {                
                var d = this.deviceConfiguration.SelectedDevice.Key;
                // generic device needs to be opened and closed before their capacity are known !
                if (d.InternalKey != DeviceCaptureDriver.GenericDeviceKey)
                {
                    return;
                }
                
                this.ScannerComboBox.IsEnabled = false;

                Task.Factory.StartNew(() => this.ConfigureAnyDevice(d));                
            }
        }

        private void ConfigureAnyDevice(ILivescanDevice d)
        {
            d.DeviceSendMessage += (text, kind, capture) => this.Dispatcher.Invoke(
                (Action)(() =>
                {
                    this.DeviceMessagesLabel.Content = text;
                }));

            try
            {
                d.Sdk.Open();
                d.Open();
                d.Close();
                d.Sdk.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // nothing to do !
            }

            // refresh properties for window !
            
            

            this.Dispatcher.Invoke(
                (Action)(() =>
                {
                    this.deviceConfiguration.GenericScannerAdaptation();
                    this.ScannerComboBox.IsEnabled = true;
                    this.DeviceMessagesLabel.Content = string.Empty;
                }));
        }

        private void ButtonDeviceClose(object sender, RoutedEventArgs e)
        {
            if (this.deviceConfiguration.SelectedDevice == null)
            {
                return;
            }

            try
            {
                this.deviceConfiguration.SelectedDevice.Key.Close();                
            }
            catch (Exception)
            {
                // nothing !!
            }

            try
            {
                this.deviceConfiguration.SelectedDevice.Key.Sdk.Close();
            }
            catch (Exception)
            {
                // nothing
            }
        }

        private void ButtonDeviceExternalTool(object sender, RoutedEventArgs e)
        {
            if (this.deviceConfiguration.SelectedDevice == null)
            {
                return;
            }

            var device = this.deviceConfiguration.SelectedDevice.Key;

            if (string.IsNullOrEmpty(device.ExternalTool))
            {
                return;
            }

            if (device.IsOpened || device.Sdk.IsOpened)
            {
                ButtonDeviceClose(sender, null);
            }

            string localPath = new Uri(device.GetType().Assembly.GetName().CodeBase).LocalPath;
            var dir = Path.GetDirectoryName(localPath);
            var path = Path.Combine(dir, device.ExternalTool);

            
            this.externalProcess = new Process { EnableRaisingEvents = true };
            this.externalProcess.Exited += (o, args) => 
                Application.Current.Dispatcher.Invoke(new Action(() => this.SetWindowsState(WindowState.Normal)));
            this.externalProcess.StartInfo.FileName = path;
            if (this.externalProcess.Start())
            {
                this.SetWindowsState(WindowState.Minimized);
            }
        }

        private void SetWindowsState(WindowState newState)
        {

            foreach (Window window in Application.Current.Windows)
            {
                window.WindowState = newState;
            }
        }

        private void ButtonAutoDetectClick(object sender, RoutedEventArgs e)
        {
            var list = this.deviceConfiguration.DeviceList;
            var sdks = list.Select(x => x.Key.Sdk).Distinct();

            foreach (var captureSdk in sdks)
            {
                try
                {
                    captureSdk.Open();
                    var pluggedDevice = captureSdk.GetPluggedDevices().ToList();

                    if (pluggedDevice.Any())
                    {
                        var selectedDevice = pluggedDevice[0];
                        this.deviceConfiguration.SelectedDevice = list.FirstOrDefault(x => x.Key == selectedDevice);
                        return;
                    }

                    captureSdk.Close();
                }
                catch (Exception ex)
                {
                    // nothing !!
                    Console.WriteLine(ex);
                }
            }


        }

        private void ParameterDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var lbl = sender as System.Windows.Controls.Label;
            
            if (lbl == null)
            {
                return;
            }

            var prop = lbl.Tag as DeviceProperty;

            if (prop == null)
            {
                return;
            }
            
            var dev = this.DeviceConfiguration.SelectedDevice.Key;
            var cProp = dev.Properties.GetProperty(prop.Key);

            dev.Properties.SetValue(cProp.InternalKey, cProp.DefaultValue);
            prop.Value = prop.Value;
        }
    }
}
