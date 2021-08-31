// -----------------------------------------------------------------------
// <copyright file="ListElement.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace XL_ID.Wpf.ViewModel
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ListElementViewModel<TValueType>
    {

        public ListElementViewModel(TValueType key, string label)
        {
            this.Key = key;
            this.Label = label;
            
        }

        public TValueType Key { get; set; }

        public string Label { get; set; }        
    }
}
