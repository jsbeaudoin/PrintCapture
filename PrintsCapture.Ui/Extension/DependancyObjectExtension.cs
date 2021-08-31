// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DependencyObjectExtension.cs" company="Solutions Xl-ID inc.">
//   update text
// </copyright>
// <summary>
//   Defines the DependencyObjectExtension type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PrintsCapture.Ui.Extension
{
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// The dependency object extension.
    /// </summary>
    public static class DependencyObjectExtension
    {
        /// <summary>
        /// Gets the child of given type from the object.
        /// </summary>
        /// <param name="depObj">
        /// The dependency object.
        /// </param>
        /// <typeparam name="T">
        /// Type of the wanted child
        /// </typeparam>
        /// <returns>
        /// instance of <see cref="T"/> from the child object.
        /// </returns>
        public static T GetChildOfType<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
            {
                return null;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}