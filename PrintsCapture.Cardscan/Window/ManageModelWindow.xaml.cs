using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PrintsCapture.Cardscan.ViewModel;
using PrintsCapture.Prints.Language;
using XL_ID.Utilities.Wpf.Extension;

namespace PrintsCapture.Cardscan.Window
{
    /// <summary>
    /// Interaction logic for ManageModelWindow.xaml
    /// </summary>
    public partial class ManageModelWindow : System.Windows.Window, INotifyPropertyChanged
    {
        private bool closedByButton = false;
        

        public ModelNameManagerViewModel ViewModel { get; set; }

        public ManageModelWindow(ModelNameManagerViewModel viewModel)
        {
            this.ViewModel = viewModel;                        

            InitializeComponent();
            
            if (this.ViewModel.IsReadOnly || !this.ViewModel.IsCreated)
            {
                this.OverrideRadioButton.IsEnabled = false;
                this.CreateNewRadioButton.IsChecked = true;
            }

            var bind = this.NewNameTextBox.GetBindingExpression(TextBox.TextProperty).ParentBinding;

            bind.ValidationRules.Add(new ModelNameValidationRule() { OriginalName = viewModel.Name});

            this.Closed += this.OnClosed;

            if (this.NewNameTextBox.IsEnabled)
            {
                // Put the focus on the textbox or password box !
                this.Loaded += (sender, args) => this.NewNameTextBox.Focus();
            }
        }

        private void OnClosed(object sender, EventArgs eventArgs)
        {
            if (this.closedByButton) return;

            this.ViewModel.UserAction = UserAction.None;
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
        

        private void CloseDialog(UserAction userActionToTake)
        {
            this.closedByButton = true;
            this.ViewModel.UserAction = userActionToTake;
            this.Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.CloseDialog(UserAction.None);
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            
            this.NewNameTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();

            if (!this.HasNoValidationError())
            {
                return;
            }

            var isChecked = this.OverrideRadioButton.IsChecked.HasValue && this.OverrideRadioButton.IsChecked.Value;
            var action = isChecked ? UserAction.Override : UserAction.CreateNew;
            this.CloseDialog(action);
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            this.CloseDialog(UserAction.Delete);
        }

        private void NewModelChecked(object sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
            if (chk == null || !chk.IsChecked.HasValue || !chk.IsChecked.Value)
            {
                return;
            }

            this.NewNameTextBox.Focus();
        }

        private void NewNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {            
            // select the enitre text
            this.NewNameTextBox.SelectAll();            
        }

        private void NewModelClicked(object sender, RoutedEventArgs e)
        {
            if (this.CreateNewRadioButton.IsChecked == true)
            {
                this.NewNameTextBox.Focus();
            }
        }
    }

    public class ModelNameValidationRule : ValidationRule
    {
        public string OriginalName { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var txt = value as string;

            if (txt == null || string.IsNullOrEmpty(txt) || txt == this.OriginalName)
            {
                return new ValidationResult(false, CommonText.InvalidCharacters);
            }            

            return new ValidationResult(true, null);
        }
    }
}
