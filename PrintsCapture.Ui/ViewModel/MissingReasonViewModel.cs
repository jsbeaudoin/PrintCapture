namespace PrintsCapture.Ui.ViewModel
{
    using System.ComponentModel;

    public class MissingReasonViewModel : INotifyPropertyChanged
    {
        private bool isSelected;

        public string Label { get; set; }

        public string Code { get; set; }

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }
            set
            {
                if (this.isSelected == value)
                {
                    return;
                }
                this.isSelected = value;
                this.OnPropertyChanged(@"IsSelected");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
