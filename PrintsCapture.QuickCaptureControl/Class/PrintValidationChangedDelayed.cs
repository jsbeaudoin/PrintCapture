namespace PrintsCapture.QuickCaptureControl.Class
{
    using System;

    using PrintsCapture.Prints;

    public class PrintValidationChangedDelayed
    {
        private const int DelayedNotificationInterval = 1200;
        private readonly DelayedOperation delayedOperation;

        public PrintList List { get; private set; }

        /// <summary>
        /// When the Rule object properties are changed, after last changed and a delay the event is raised. This allow to change multiple values without changing on the fly.
        /// </summary>
        public event EventHandler DelayedRuleChanged
        {
            add { this.delayedOperation.OperationCalled += value; }
            remove { this.delayedOperation.OperationCalled -= value; }
        }

        public PrintValidationChangedDelayed(PrintList list)
        {
            this.List = list;            
            this.delayedOperation = new DelayedOperation(DelayedNotificationInterval);            
        }

        public bool TimerEnabled
        {
            get
            {
                return this.delayedOperation.Enabled;
            }
            set
            {
                this.delayedOperation.Enabled = value;                
            }
        }

        public int SequenceThreshold
        {
            get
            {
                return this.List.Rules.SequenceThreshold;
            }
            set
            {
                if (Equals(value, this.List.Rules.SequenceThreshold))
                {
                    return;
                }
                this.List.Rules.SequenceThreshold = value;                
                SequenceCheckToGaugeConverter.SequenceThreshold = value;
                
                this.NotifyChange();
            }
        }

        public bool IsSequenceEnabled
        {
            get
            {
                return this.List.Rules.IsSequenceEnabled;
            }
            set
            {                
                if (value == this.List.Rules.IsSequenceEnabled)
                {
                    return;
                }
                this.List.Rules.IsSequenceEnabled = value;
                
                this.NotifyChange();
            }
        }

        public bool IsSequencePositionEnabled
        {
            get
            {
                return this.List.Rules.IsSequenceChangingPosition;
            }
            set
            {
                if (Equals(value, this.List.Rules.IsSequenceChangingPosition))
                {
                    return;
                }
                
                this.List.Rules.IsSequenceChangingPosition = value;                
                this.NotifyChange();
            }
        }

        public int QualityThreshold
        {
            get
            {
                return this.List.Rules.QualityThreshold;
            }
            set
            {
                if (value == this.List.Rules.QualityThreshold)
                {
                    return;
                }
                this.List.Rules.QualityThreshold = value; 
                this.NotifyChange();
            }
        }

        public bool IsQualityEnabled
        {
            get
            {
                return !this.List.Rules.IsQualityEnabled;
            }
            set
            {                
                if (value == this.List.Rules.IsQualityEnabled)
                {
                    return;
                }
                this.List.Rules.IsQualityEnabled = value;   
                this.NotifyChange();
            }
        }        

        private void NotifyChange()
        {
            this.delayedOperation.DoOperation();            
        }
        
    }
}
