using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XL_ID.Wpf.Extension
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public static class DependancyObjectExtension
    {
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }            
        }

        public static bool HasNoValidationError(this DependencyObject obj)
        {            
            // The dependency object is valid if it has no errors and all
            // of its children (that are dependency objects) are error-free.

            // mm 2015-03-03
            // each framework element must be enabled for validation to matter !
            return !Validation.GetHasError(obj) &&
            LogicalTreeHelper.GetChildren(obj)
            .OfType<DependencyObject>()
            .All(x => HasNoValidationError(x) || !IsEnabled(x));
        }

        public static bool IsEnabled(object o)
        {
            var fe = o as FrameworkElement;
            return (fe != null && fe.IsEnabled);
        }
    }
}
