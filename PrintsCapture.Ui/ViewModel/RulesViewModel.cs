using PrintsCapture.Ui.Annotations;

namespace PrintsCapture.Ui.ViewModel
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Text;

    using Prints.Enum;
    using Converter;
    using PrintsCapture.Ui.Language;

    public class RulesViewModel : INotifyPropertyChanged
    {
        private string ruleStatusText;

        private int sequenceThreshold;

        private bool isSequenceEnabled;

        private bool isSequencePositionEnabled;

        private int qualityThreshold;

        private bool isQualityEnabled;       

        private bool isOverrideRolledForbidden;

        private bool isOverrideFlatForbidden;

        private bool isDataCompressed;

        private PrintCaptureGroup captureMode;

        private int cropTolerance;

        private bool isOverrideAlwaysShown;
        private int retryNeededForOverride;

        private int minimumMinutiaCount;

        public PrintCaptureGroup CaptureMode
        {
            get
            {
                return this.captureMode;
            }
            set
            {
                if (value == this.captureMode)
                {
                    return;
                }
                this.captureMode = value;
                this.OnPropertyChanged("CaptureMode");
            }
        }

        public bool IsFlatCaptureMode => this.captureMode == PrintCaptureGroup.FlatOnly;

        public int SequenceThreshold
        {
            get
            {
                return this.sequenceThreshold;
            }
            set
            {
                if (Equals(value, this.sequenceThreshold))
                {
                    return;
                }
                this.sequenceThreshold = value;
                this.OnPropertyChanged("SequenceThreshold");

                SequenceCheckToGaugeConverter.SequenceThreshold = value;

                this.BuildRuleStatus();
            }
        }

        public bool IsSequenceEnabled
        {
            get
            {
                return this.isSequenceEnabled;
            }
            set
            {
                if (Equals(value, this.isSequenceEnabled))
                {
                    return;
                }
                this.isSequenceEnabled = value;
                this.OnPropertyChanged("IsSequenceEnabled");

                this.BuildRuleStatus();
            }
        }

        public bool IsSequencePositionEnabled
        {
            get
            {
                return this.isSequencePositionEnabled;
            }
            set
            {
                if (Equals(value, this.isSequencePositionEnabled))
                {
                    return;
                }
                this.isSequencePositionEnabled = value;
                this.OnPropertyChanged("IsSequencePositionEnabled");

                this.BuildRuleStatus();
            }
        }

        public int QualityThreshold
        {
            get
            {
                return this.qualityThreshold;
            }
            set
            {
                if (value == this.qualityThreshold)
                {
                    return;
                }
                this.qualityThreshold = value;
                this.OnPropertyChanged("QualityThreshold");                
            }
        }

        public int CropTolerance
        {
            get
            {
                return this.cropTolerance;
            }
            set
            {
                if (value == this.cropTolerance)
                {
                    return;
                }
                this.cropTolerance = value;
                this.OnPropertyChanged("CropTolerance");
            }
        }

        public bool IsQualityEnabled
        {
            get
            {
                return this.isQualityEnabled;
            }
            set
            {
                if (value.Equals(this.isQualityEnabled))
                {
                    return;
                }
                this.isQualityEnabled = value;
                this.OnPropertyChanged("IsQualityEnabled");                
            }
        }

        public bool IsOverrideFlatForbidden
        {
            get
            {
                return this.isOverrideFlatForbidden;
            }
            set
            {
                if (value.Equals(this.isOverrideFlatForbidden))
                {
                    return;
                }
                this.isOverrideFlatForbidden = value;
                this.OnPropertyChanged("IsOverrideFlatForbidden");
            }
        }

        public bool IsOverrideRolledForbidden
        {
            get
            {
                return this.isOverrideRolledForbidden;
            }
            set
            {
                if (value.Equals(this.isOverrideRolledForbidden))
                {
                    return;
                }
                this.isOverrideRolledForbidden = value;
                this.OnPropertyChanged("IsOverrideRolledForbidden");
            }
        }

        public int RetryNeededForOverride
        {
            get { return retryNeededForOverride; }
            set
            {
                if (value == retryNeededForOverride) return;
                retryNeededForOverride = value;
                OnPropertyChanged("RetryNeededForOverride");
            }
        }

        public bool IsDataCompressed
        {
            get
            {
                return this.isDataCompressed;
            }
            set
            {
                if (value.Equals(this.isDataCompressed))
                {
                    return;
                }
                this.isDataCompressed = value;
                this.OnPropertyChanged("IsDataCompressed");

                this.BuildRuleStatus();
            }
        }

        public bool IsOverrideAlwaysShown
        {
            get { return isOverrideAlwaysShown; }
            set
            {
                if (value == this.isOverrideAlwaysShown)
                {
                    return;
                }
                isOverrideAlwaysShown = value;
                this.OnPropertyChanged("IsOverrideAlwaysShown");
            }
        }        

        public int MinimumMinutiaCount
        {
            get
            {
                return this.minimumMinutiaCount;
            }
            set
            {
                if (value == this.minimumMinutiaCount)
                {
                    return;
                }
                this.minimumMinutiaCount = value;
                this.OnPropertyChanged(@"MinimumMinutiaCount");
            }
        }

        public string RuleStatusText
        {
            get
            {
                return this.ruleStatusText;
            }
            set
            {
                if (value == this.ruleStatusText)
                {
                    return;
                }
                this.ruleStatusText = value;
                this.OnPropertyChanged("RuleStatusText");
            }
        }

        public RulesViewModel(Prints.PrintRules printRules)
        {
            this.captureMode = printRules.CaptureGroup;
            this.isDataCompressed = printRules.IsDataCompressed;
            this.isOverrideFlatForbidden = printRules.IsOverrideFlatForbidden;
            this.isOverrideRolledForbidden = printRules.IsOverrideRolledForbidden;
            this.isQualityEnabled = printRules.IsQualityEnabled;
            this.isSequenceEnabled = printRules.IsSequenceEnabled;
            this.isSequencePositionEnabled = printRules.IsSequenceChangingPosition;
            this.qualityThreshold = printRules.QualityThreshold;
            this.sequenceThreshold = printRules.SequenceThreshold;
            this.cropTolerance = printRules.CropTolerance;           
            this.minimumMinutiaCount = printRules.MinimumMinitiaCount;

            this.BuildRuleStatus();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BuildRuleStatus()
        {
            var sb = new StringBuilder();

            //  Verify Sequence check score and Enabled
            if (!this.IsSequenceEnabled)
            {
                sb.Append(Text.RulesSequenceDisabled);
            }
            else
            {
                if (this.SequenceThreshold != 50)
                {
                    sb.Append(Text.RulesSequenceThreshold);
                    sb.Append(' ');
                    sb.Append(this.SequenceThreshold);
                }
            }

            if (this.IsSequencePositionEnabled)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(Text.RulesSequenceRepositionEnabled);
            }

            // Verify Quality Check
            if (this.IsQualityEnabled)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(Text.RulesQualityEnabled);
                sb.Append(' ');
                sb.Append(this.QualityThreshold);
            }


            // VERIFY IS DATA COMPRESSED
            if (!this.IsDataCompressed)
            {
                if (sb.Length > 0) sb.Append(", ");

                sb.Append(Text.RulesDataCompressionDisabled);
            }

            this.RuleStatusText = sb.Length > 0 ? sb.ToString() : null;

        }
    }
}
