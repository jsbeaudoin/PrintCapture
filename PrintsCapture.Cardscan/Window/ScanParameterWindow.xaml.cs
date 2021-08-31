using PrintsCapture.Prints.Language;

namespace PrintsCapture.Cardscan.Window
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;


    using PrintsCapture.Cardscan.ViewModel;

    /// <summary>
    /// Interaction logic for ScanParameterWindow.xaml
    /// </summary>
    public partial class ScanParameterWindow : INotifyPropertyChanged
    {
        private ScanParameterViewModel viewModel;

        public ScanParameterWindow(ScanParameterViewModel viewModel)
        {
            this.ViewModel = viewModel;

            InitializeComponent();

            
        }

        public ScanParameterViewModel ViewModel
        {
            get
            {
                return this.viewModel;
            }
            set
            {
                if (Equals(value, this.viewModel))
                {
                    return;
                }
                this.viewModel = value;
                this.OnPropertyChanged("ViewModel");                
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {            
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));            
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.IsCustom)
            {
                if (this.ViewModel.CustomOffset < 0 || this.ViewModel.CustomOffset > 95)
                {
                    MessageBox.Show(CommonText.ValidScanOffsetValues, CommonText.ValidationError, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (this.ViewModel.CustomSize < 5 || this.ViewModel.CustomSize > 100)
                {
                    MessageBox.Show(CommonText.ValidScanSizeValues, CommonText.ValidationError, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (this.ViewModel.CustomOffset + this.viewModel.CustomSize > 100)
                {
                    MessageBox.Show(CommonText.InvalidScanSizeAndOffset, CommonText.ValidationError, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                CardScanSettings.Default.PresetManualOffset = (float)Math.Round(this.ViewModel.CustomOffset / 100F, 2);
                CardScanSettings.Default.PresetManualSize = (float)Math.Round(this.ViewModel.CustomSize / 100F, 2);                
            }

            this.DialogResult = true;
            this.Close();
        }

        private void ScanZoneRadioInitialized(object sender, EventArgs e)
        {
            var rb = (RadioButton)sender;
            int presetCode;
            if (rb.Tag == null)
            {
                return;
            }

            int.TryParse(rb.Tag.ToString(), out presetCode);

            if (presetCode == this.viewModel.PresetCode || (presetCode > 0 && this.viewModel.PresetCode > 0))
            {
                rb.IsChecked = true;
            }
        }        

        private void ScanZoneRadioChecked(object sender, RoutedEventArgs e)
        {
            var rb = (RadioButton)sender;
            int presetCode;
            int.TryParse(rb.Tag.ToString(), out presetCode);

            if (rb.IsChecked.HasValue && rb.IsChecked.Value && presetCode < this.ViewModel.PresetCode)
            {
                this.ViewModel.PresetCode = presetCode;
            }
        }       
    }
}
