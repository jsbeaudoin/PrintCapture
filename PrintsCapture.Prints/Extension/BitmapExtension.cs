namespace PrintsCapture.Prints.Extension
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using NLog;

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
        /// <summary>
        /// Creates a deep clone, retaining the ImageFormat, color indexes and resolution of the image
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Bitmap DeepClone(this Bitmap img)
        {
            if (img == null)
            {
                return null;
            }

            var clone = (Bitmap)img.Clone();
            clone.SetResolution(img.HorizontalResolution, img.VerticalResolution);

            return clone;
        }

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
                    BitmapSource bmpSource = null;
                    IntPtr hBitmap = IntPtr.Zero;

                    try
                    {
                        hBitmap = img.GetHbitmap();

                        var szOptions = BitmapSizeOptions.FromWidthAndHeight((int)newWidth, (int)newHeight);

                        bmpSource = Imaging.CreateBitmapSourceFromHBitmap(
                            hBitmap,
                            IntPtr.Zero,
                            Int32Rect.Empty,
                            szOptions);
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetCurrentClassLogger().Error(ex, "ToImageSource : Can't create BitmapSource");
                        return null;
                    }
                    finally
                    {
                        if (hBitmap != IntPtr.Zero)
                        {
                            NativeMethods.DeleteObject(hBitmap);
                        }
                        
                    }
                    
                    //if (deleteHBitmap)
                    //{
                    //    try
                    //    {
                    //        NativeMethods.DeleteObject(hBitmap);
                    //    }
                    //    catch (Exception)
                    //    {
                    //        // nothing
                    //    }                        
                    //}

                    return bmpSource;
                });
            
            try
            {
                // Create the bmpSource on the same thread that it will be used on ...
                return (ImageSource)Application.Current.Dispatcher.Invoke(bmpSourceCreation);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, "ToImageSource : Can't create BitmapSource");
                return null;
            }
                        
        }
    }
}