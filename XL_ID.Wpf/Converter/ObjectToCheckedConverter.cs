// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectToCheckedConverter.cs" company="Solution XL-ID">
//   update text.
// </copyright>
// <summary>
//   Object to checked converter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace XL_ID.Wpf.Converter
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// Object to checked converter.
    /// </summary>
    public class ObjectToCheckedConverter : IValueConverter
    {
        /// <summary>
        /// The convert method
        /// </summary>
        /// <param name="value">
        /// The affected value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The linked code parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && parameter == null)
            {
                return true;
            }

            if (value == null || parameter == null)
            {
                return false;
            }

            var ctrl = parameter as Control;

            if (ctrl != null)
            {
                return ctrl.Tag.Equals(value);
            }

            return value.Equals(parameter);
        }

        /// <summary>
        /// The convert back method.
        /// </summary>
        /// <param name="value">
        /// The affected value (Checked or not).
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The linked code parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var checkedControl = (bool?)value;
            var boundValue = parameter;

            if (checkedControl == null || !checkedControl.Value)
            {
                return Binding.DoNothing;
            }

            var ctrl = boundValue as Control;

            if (ctrl != null)
            {
                return ctrl.Tag;
            }

            return boundValue;
        }
    }
}