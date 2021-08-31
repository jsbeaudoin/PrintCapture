namespace PrintsCapture.Ui.UserControls
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls.Ribbon;

    public class ToggleGroupHandler
    {
        private RibbonToggleButton checkedButton = null;

        private readonly List<RibbonToggleButton> groupMembers = new List<RibbonToggleButton>();

        public void Add(RibbonToggleButton toggleButton)
        {
            this.groupMembers.Add(toggleButton);

            if (toggleButton.IsChecked.HasValue && toggleButton.IsChecked.Value)
            {
                this.checkedButton = toggleButton;
            }

            toggleButton.Checked += this.ToggleButtonOnChecked;
            toggleButton.Unchecked += this.ToggleButtonOnUnchecked;
        }

        public void CheckByValue(object tagValue)
        {

            foreach (var toggleButton in this.groupMembers)
            {
                if (Equals(tagValue, toggleButton.Tag))
                {
                    toggleButton.IsChecked = true;
                }
            }

        }

        public object GetCheckedValue()
        {
            if (this.checkedButton == null)
            {
                return null;
            }

            return (from toggleButton in this.groupMembers where toggleButton.IsChecked.HasValue && toggleButton.IsChecked.Value select toggleButton.Tag).FirstOrDefault();
        }

        private void ToggleButtonOnUnchecked(object sender, RoutedEventArgs routedEventArgs)
        {
            var tg = (RibbonToggleButton)sender;

            if (tg == this.checkedButton || this.checkedButton == null)
            {
                tg.IsChecked = true;
            }
        }

        private void ToggleButtonOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            var tg = (RibbonToggleButton)sender;

            this.checkedButton = tg;

            this.UnCheckAll();
        }


        private void UnCheckAll()
        {            

            foreach (var toggleButton in this.groupMembers)
            {
                if (this.checkedButton == null && toggleButton.IsChecked.HasValue && toggleButton.IsChecked.Value)
                {
                    this.checkedButton = toggleButton;
                }
                else if (toggleButton.IsChecked.HasValue && toggleButton.IsChecked.Value && this.checkedButton != toggleButton)
                {
                    toggleButton.IsChecked = false;
                }
            }
        }
    }
}
