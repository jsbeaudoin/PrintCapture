// -----------------------------------------------------------------------
// <copyright file="ColorReplacer.cs" company="XL-ID">
// Replacement of a base color with a tolerance level to another color
// </copyright>
// -----------------------------------------------------------------------

namespace PrintsCapture.Cardscan
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Class that analyzes an image and replaces a given color for another if the pixel color is near enough
    /// </summary>
    public class ColorReplacer
    {
        /// <summary>
        /// The color range of minimal and maximal values for Red Green and Blue components.
        /// </summary>
        private ColorRange colorRange;

        /// <summary>
        /// Replaces the color.
        /// </summary>
        /// <param name="baseColor">Base Color.</param>
        /// <param name="tolerance">The tolerance value.</param>
        /// <param name="newColor">The new color to replace all near enough pixel.</param>
        /// <param name="image">The image to look into and modify.</param>
        public void ReplaceColor(Color baseColor, byte tolerance, Color newColor, Bitmap image)
        {
            this.colorRange = new ColorRange(baseColor, tolerance);
            var optimized = new OptimizedBitmap(image);

            for (var x = 0; x < image.Width; x++)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    var pixColor = optimized.GetPixel(x, y);
                    if (this.colorRange.IsNear(pixColor))
                    {
                        optimized.SetPixel(x, y, newColor);
                    }
                }
            }

            optimized.UnlockImage();
        }        

        private class OptimizedBitmap
        {
            private BitmapData bmpData;
            private byte[] bmpBytes;

            private int bmpStride;

            private int pixelFormatSize = 3;

            private bool usePalette = false;

            private bool invertedImage;

            private int paletteColorIndex = -1;

            private Color[] colorPalette;

            public OptimizedBitmap(Bitmap source)
            {
                this.Bmp = source;

                this.bmpData = source.LockBits(
                    new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadWrite, source.PixelFormat);

                this.bmpStride = Math.Abs(this.bmpData.Stride);

                this.pixelFormatSize = Image.GetPixelFormatSize(source.PixelFormat) / 8;

                this.bmpBytes = new byte[this.bmpStride * source.Height];

                Marshal.Copy(this.bmpData.Scan0, this.bmpBytes, 0, this.bmpBytes.Length);

                this.invertedImage = this.bmpStride != this.bmpData.Stride;

                this.colorPalette = this.Bmp.Palette.Entries;

                this.usePalette = source.PixelFormat == PixelFormat.Format1bppIndexed
                                   || source.PixelFormat == PixelFormat.Format4bppIndexed
                                   || source.PixelFormat == PixelFormat.Format8bppIndexed;
            }

            public Bitmap Bmp { get; private set; }

            public Color GetPixel(int x, int y)
            {
                var pos = this.GetArrayPosition(x, y);
                if (this.usePalette)
                {
                    var paletteIndex = this.GetPaletteIndex(pos);
                    return this.colorPalette[paletteIndex];
                }
                else if (this.pixelFormatSize == 3)
                {
                    byte pixelB = this.bmpBytes[pos];
                    byte pixelR = this.bmpBytes[pos + 2];
                    byte pixelG = this.bmpBytes[pos + 1];
                    return Color.FromArgb(pixelR, pixelG, pixelB);
                }
                else if (this.pixelFormatSize == 4)
                {
                    byte alphaChannel = this.bmpBytes[pos + 3];
                    byte pixelB = this.bmpBytes[pos + 0];
                    byte pixelR = this.bmpBytes[pos + 2];
                    byte pixelG = this.bmpBytes[pos + 1];
                    return Color.FromArgb(alphaChannel, pixelR, pixelG, pixelB);
                }

                throw new ApplicationException("Unknown Image Information");
            }

            public void SetPixel(int x, int y, Color color)
            {
                var pos = this.GetArrayPosition(x, y);
                if (this.usePalette)
                {
                    var targetPaletteIndex = this.GetPaletteColorIndex(color);
                    this.SetPaletteIndex(pos, targetPaletteIndex);

                    //var paletteIndex = this.GetPaletteIndex(pos);
                    //this.Bmp.Palette.Entries[paletteIndex] = color;
                }
                else if (this.pixelFormatSize == 3)
                {
                    this.bmpBytes[pos] = color.B;
                    this.bmpBytes[pos + 2] = color.R;
                    this.bmpBytes[pos + 1] = color.G;
                }
                else if (this.pixelFormatSize == 4)
                {
                    this.bmpBytes[pos + 3] = color.A;
                    this.bmpBytes[pos + 0] = color.B;
                    this.bmpBytes[pos + 2] = color.R;
                    this.bmpBytes[pos + 1] = color.G;
                }
                else
                {
                    throw new ApplicationException("Unknown Image Information");
                }
            }

            public void UnlockImage()
            {
                Marshal.Copy(this.bmpBytes, 0, this.bmpData.Scan0, this.bmpBytes.Length);
                this.Bmp.UnlockBits(this.bmpData);
            }

            private int GetPaletteIndex(int arrayposition)
            {
                var byteData = new byte[4];

                for (var x = 0; x < this.pixelFormatSize; x++)
                {
                    byteData[x] = this.bmpBytes[arrayposition + x];
                }

                return BitConverter.ToInt32(byteData, 0);
            }

            private void SetPaletteIndex(int arrayposition, int index)
            {
                var byteData = BitConverter.GetBytes(index);

                for (var x = 0; x < this.pixelFormatSize; x++)
                {
                    this.bmpBytes[arrayposition + x] = byteData[x];
                }
            }

            private int GetPaletteColorIndex(Color color)
            {
                int colorValue = color.ToArgb();

                if (this.paletteColorIndex > 0 && this.colorPalette[this.paletteColorIndex].ToArgb() == colorValue)
                {
                    return this.paletteColorIndex;
                }

                int length = this.Bmp.Palette.Entries.Length;
                int lastColorValue = -2;
                int currentColorValue = -2;
                int indexToInsert = -1;

                for (int index = 0; index < length; index++)
                {
                    lastColorValue = currentColorValue;
                    currentColorValue = this.colorPalette[index].ToArgb();
                    if (currentColorValue == colorValue)
                    {
                        this.paletteColorIndex = index;
                        return index;
                    }
                    else if (lastColorValue == currentColorValue)
                    {
                        indexToInsert = index - 1;
                        break;
                    }
                }

                // if palette entries have unused entries, use one for wanted color
                if (indexToInsert == -1 || lastColorValue != currentColorValue)
                {
                    throw new ApplicationException("Image palette does not contain Color " + color);
                }
                else
                {
                    // Modify the palette
                    ColorPalette palette = this.Bmp.Palette;
                    Color[] entries = palette.Entries;

                    entries[indexToInsert] = color;
                    this.Bmp.Palette = palette; // The crucial statement

                    this.paletteColorIndex = indexToInsert;

                    return this.paletteColorIndex;
                }
            }

            private int GetArrayPosition(int x, int y)
            {
                if (this.invertedImage)
                {
                    var vert = this.bmpStride * (this.bmpData.Height - 1 - y); // Stride include bitmap pixel size
                    var hor = (this.bmpData.Width - 1 - x) * this.pixelFormatSize;
                    return vert + hor;
                }
                else
                {
                    var vert = y * this.bmpStride; // Stride included Bitmap pizel size
                    var hor = x * this.pixelFormatSize;
                    return vert + hor;
                }
            }
        }

        /// <summary>
        /// Class to have a Min and Max value for color components
        /// </summary>
        private class Range
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Range"/> class.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="tolerance">The tolerance.</param>
            public Range(byte value, byte tolerance)
            {
                if (value + tolerance > byte.MaxValue)
                {
                    this.Max = byte.MaxValue;
                }
                else
                {
                    this.Max = (byte)(value + tolerance);
                }

                if (value - tolerance < byte.MinValue)
                {
                    this.Min = byte.MinValue;
                }
                else
                {
                    this.Min = (byte)(value - tolerance);
                }
            }

            /// <summary>
            /// Gets the minimum.
            /// </summary>
            /// <value>
            /// The minimum.
            /// </value>
            public byte Min { get; private set; }

            /// <summary>
            /// Gets the maximum.
            /// </summary>
            /// <value>
            /// The maximum.
            /// </value>
            public byte Max { get; private set; }
        }

        /// <summary>
        /// Color Range manager
        /// </summary>
        private class ColorRange
        {
            /// <summary>
            /// The Range for the blue color component
            /// </summary>
            private Range b;

            /// <summary>
            /// The Range for the red color component
            /// </summary>
            private Range r;

            /// <summary>
            /// The Range for the green color component
            /// </summary>
            private Range g;

            /// <summary>
            /// Initializes a new instance of the <see cref="ColorRange"/> class.
            /// </summary>
            /// <param name="color">The color.</param>
            /// <param name="tolerance">The tolerance.</param>
            public ColorRange(Color color, byte tolerance)
            {
                this.b = new Range(color.B, tolerance);
                this.r = new Range(color.R, tolerance);
                this.g = new Range(color.G, tolerance);
            }

            /// <summary>
            /// Determines whether the specified color is near enough.
            /// </summary>
            /// <param name="color">The color.</param>
            /// <returns>Boolean to indicate if the color is considered near enough.</returns>
            public bool IsNear(Color color)
            {
                if (color.R >= this.r.Min && color.R <= this.r.Max &&
                    color.G >= this.g.Min && color.G <= this.g.Max &&
                    color.B >= this.b.Min && color.B <= this.b.Max)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
