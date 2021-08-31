namespace PrintsCapture.Ui.ViewModel
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;   

    using PrintsCapture.Prints.ViewModel;    

    public class EndorsementFingerCaptureViewModel : INotifyPropertyChanged
    {
        private EndorsableFingerViewModel newFinger;        

        private List<EndorsableFingerViewModel> endorsableFingers;                  

        public List<EndorsableFingerViewModel> EndorsableFingers
        {
            get
            {
                return this.endorsableFingers;
            }
            set
            {
                if (Equals(value, this.endorsableFingers))
                {
                    return;
                }
                this.endorsableFingers = value;
                this.OnPropertyChanged("EndorsableFingers");

                if (value == null || value.Count == 0)
                {
                    return;
                }

                this.NewFinger = value.First();
            }
        }        

        public EndorsableFingerViewModel NewFinger
        {
            get
            {
                return this.newFinger;
            }
            set
            {
                if (Equals(value, this.newFinger))
                {
                    return;
                }
                this.newFinger = value;
                this.OnPropertyChanged("NewFinger");
            }
        }        

        public event PropertyChangedEventHandler PropertyChanged;

        //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }        
    }
}