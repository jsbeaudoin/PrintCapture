using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XL_ID.Wpf.WindowHelper
{
    using System.Windows;

    public static class WindowHelper
    {

        public static Window GetWindowByTag(object tag)
        {
            var a = Application.Current;

            return a.Windows.Cast<Window>().FirstOrDefault(x => x.Tag != null && x.Tag.Equals(tag));            
        }

        public static Window GetWindowByName(string name)
        {
            var a = Application.Current;

            return a.Windows.Cast<Window>().FirstOrDefault(x => x.Name == name);
        }

        public static void SetWindowFullScreen(Window w)
        {
            w.Left = 0;
            w.Top = 0;
            w.Width = SystemParameters.WorkArea.Width;
            w.Height = SystemParameters.WorkArea.Height;            
        }

    }
}
