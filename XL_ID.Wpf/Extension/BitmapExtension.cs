namespace XL_ID.Wpf.Extension
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// FxCop requires all Marshalled functions to be in a class called NativeMethods.
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);
    }

    public static class BitmapExtension
    {

        public static ImageSource ToImageSource(this Bitmap img, bool deleteHBitmap = false, bool mapToWpfDpi = true)
        {
            const int WpfDpi = 96;

            if (img == null)
            {
                return null;
            }

            var bmpSourceCreation = new Func<ImageSource>(
                () =>
                {
                    var newWidth = mapToWpfDpi ? img.Width / img.HorizontalResolution * WpfDpi : img.Width;
                    var newHeight = mapToWpfDpi ? img.Height / img.VerticalResolution * WpfDpi : img.Height;
                    var hBitmap = img.GetHbitmap();

                    var szOptions = BitmapSizeOptions.FromWidthAndHeight((int)newWidth,(int)newHeight);

                    var bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        szOptions);

                    if (deleteHBitmap)
                    {
                        NativeMethods.DeleteObject(hBitmap);
                    }

                    return bmpSource;
                });
            
            try
            {
                // Create the bmpSource on the same thread that it will be used on ...
                return (ImageSource)Application.Current.Dispatcher.Invoke(bmpSourceCreation);
            }
            catch 
            {                
                return null;
            }
                        
        }                

    }

    
}