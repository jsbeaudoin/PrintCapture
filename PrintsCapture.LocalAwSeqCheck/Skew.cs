using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintsCapture.LocalAwSeqCheck
{
    using System.Drawing;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Security.Cryptography;

    using PrintsCapture.Prints;

    //  Public Class main
  //    Public Shared Sub Main()
  //        Dim fnIn As String = "d:\skewsample_in.tif"
  //        Dim fnOut As String = "d:\skewsample_out.tif"
  //        Dim bmpIn As New Bitmap(fnIn)
  //        Dim sk As New gmseDeskew(bmpIn)
  //        Dim skewangle As Double = sk.GetSkewAngle
  //        Dim bmpOut As Bitmap = RotateImage(bmpIn, -skewangle)
  //        bmpOut.Save(fnOut, ImageFormat.Tiff)
  //        MsgBox("Skewangle: " & skewangle)
  //    End Sub
  
  //    Private Shared Function RotateImage(ByVal bmp As Bitmap, ByVal angle As Double) As Bitmap
  //        Dim g As Graphics
  //        Dim tmp As New Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppRgb)
  
  //        tmp.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution)
  //        g = Graphics.FromImage(tmp)
  //        Try
  //            g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height)
  //            g.RotateTransform(angle)
  //            g.DrawImage(bmp, 0, 0)
  //        Finally
  //            g.Dispose()
  //        End Try
  //        Return tmp
  //    End Function
  //End Class
  
  public class gmseDeskew {
      
      /// <summary>
      /// Get the angle from the segment of the print
      /// </summary>
      /// <param name="print"></param>
      /// <param name="segment"></param>
      /// <returns></returns>
      public static int AngleFromSegment(PrintInfo print, PrintSegment segment)
      {
          // 1st --> Get Bmp !
          var bmp = new Bitmap(segment.Position.Width, segment.Position.Height);
          using (var g = Graphics.FromImage(bmp))
          {
              g.DrawImage(print.ImageForProcessing, new Rectangle(0, 0, segment.Position.Width, segment.Position.Height), segment.Position, GraphicsUnit.Pixel);
          }

          var d = new gmseDeskew(bmp);
          var angle = d.GetSkewAngle();

          return (int)angle;
      }

      /// Representation of a line in the image.
    class HougLine
    {
        // Count of points in the line.
        public int Count { get; set; }

        // Index in Matrix.
        public int Index { get; set; }

        // The line is represented as all x,y that solve y*cos(alpha)-x*sin(alpha)=d
        public double Alpha { get; set; }

        public double d { get; set; }
    }
      const int NbLines = 20;

      // The Bitmap
      private Bitmap  cBmp ;
      // The range of angles to search for lines
      public double cAlphaStart = -20;

      public double cAlphaStep = 0.2;

      public int cSteps = 40 * 5;
      // Precalculation of sin and cos.
      public double[] cSinA;

      public double[] cCosA;
      // Range of d
      public double cDMin;

      public double cDStep = 1;
      public int cDCount;
      // Count of points that fit in a line.
      public int[] cHMatrix;
  
      // Calculate the skew angle of the image cBmp.
      public double GetSkewAngle(){
          gmseDeskew.HougLine[] hl;
          int count=0;
          double sum=0;
            
          // Hough Transformation
          this.Calc();
          // Top 20 of the detected lines in the image.
          hl = this.GetTop(NbLines);
          // Average angle of the lines
          for (var i = 0; i < NbLines; i++)
          {
              sum += hl[i].Alpha;
              count += 1;
          }
          return sum / count;
      }
  
      // Calculate the Count lines in the image with most points.
      private HougLine[] GetTop(int count) {
          var hl = new HougLine[count];
          for (var i=0; i < count; i++)
          {
              hl[i] = new HougLine();
          }

          for (var i = 0; i < cHMatrix.Length; i++)
          {
              if (cHMatrix[i] > hl[count - 1].Count)
              {
                  hl[count - 1].Count = cHMatrix[i];
                  hl[count - 1].Index = i;

                  var j = count - 1;
                  while (j > 0 && hl[j].Count > hl[j - 1].Count)
                  {
                      HougLine tmp = hl[j];
                      hl[j] = hl[j - 1];
                      hl[j - 1] = tmp;
                      j -= 1;
                  }
                  
              }
          }

          for (var i = 0; i < count; i++)
          {
              int dIndex = hl[i].Index / cSteps;
              var alphaIndex = hl[i].Index - dIndex * cSteps;
              hl[i].Alpha = this.GetAlpha(alphaIndex);
              hl[i].d = dIndex + cDMin;
          }

          return hl;
      }

      public gmseDeskew(Bitmap bmp)
      {
          cBmp = bmp;
      }

      private void Calc()
      {
          var hMin = cBmp.Height / 4;
          var hMax = cBmp.Height * 3 / 4;

          this.Init();
          for (var y = hMin; y <= hMax; y++)
          {
              for (var x = 1; x < cBmp.Width - 2; x++)
              {
                  if (IsBlack(x, y) && ! this.IsBlack(x,y+1))
                  {
                      this.Calc(x, y);
                  }
              }
          }
      }

      private void Calc(int x, int y)
      {
          for (var alpha = 0; alpha < cSteps; alpha ++)
          {
              var d = y * cCosA[alpha] - x * this.cSinA[alpha];
              var dIndex = this.CalcDIndex(d);
              var index = (int)(dIndex * cSteps + alpha);
              try
              {
                  cHMatrix[index] += 1;
              }
              catch (Exception ex)
              {
                  System.Diagnostics.Debug.WriteLine(ex.Message.ToString());
              }
          }
      }

      private double CalcDIndex(double d)
      {
          return Convert.ToInt32(d - cDMin);
      }

      private double GetAlpha(int index)
      {
          return cAlphaStart + index * cAlphaStep;
      }

      // Hough Transforamtion:

      private bool IsBlack(int x, int y)
      {
          var color = cBmp.GetPixel(x, y);
          var luminance = (color.R * 0.299) + (color.G * 0.587) + (color.B * 0.114);
          return luminance < 140;
      }

      private void Init()
      {
          this.cSinA = new double[cSteps - 1];
          this.cCosA = new double[cSteps - 1];

          for (var i = 0; i < cSteps; i++)
          {
              var angle = GetAlpha(i) * Math.PI / 180.0;
              cSinA[i] = Math.Sin(angle);
              cCosA[i] = Math.Cos(angle);
          }

          cDMin = -1 * cBmp.Width;
          cDCount = (int)(2 * (cBmp.Width + cBmp.Height) / cDStep);
          cHMatrix = new int[cDCount * cSteps];
      }      
  }
}
