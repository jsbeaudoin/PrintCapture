using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using Nbis;
using XL_ID.Utilities.Image;
using XL_ID.Utilities.Log;

namespace PrintsCapture.LocalAwSeqCheck
{
    public class NbisSeqChecker
    {

        /// <summary>
        /// A print with its minutias and image, for comparison        
        /// </summary>
        /// <remarks>
        /// All prints are stoqcked, and some are generated.
        /// All generated subprints have index 900 +
        /// </remarks>
        private class Print
        {
            public Print()
            {
                this.SubPrints = new List<Print>();
            }
            
            public int Index { get; set; }

            public PrintLink Link { get; set; }

            public List<Print> SubPrints { get; set; }            

            public bool IsSubPrint { get; set; }


            public bool IsDefined { get; set; }

            public Bitmap Image { get; set; }

            public Minutia[] Minutias; 
           
            public int Quality { get; set; }

            public void Reset()
            {
                this.Minutias = null;
                this.IsDefined = false;
                this.Image = null;
                this.Quality = 0;

                foreach (var subPrint in SubPrints)
                {
                    subPrint.Reset();
                }
            }
        }

        public NbisSeqChecker()
        {
            this.GenerateBasePrints();
        }        

        private readonly Dictionary<int, Print> prints = new Dictionary<int, Print>();

        public void SetMissing(List<int> missingIndexes)
        {
            // copy the list, remove link to other class, so data cannot be modified
            foreach (var missingIndex in missingIndexes)
            {
                var p = this.GetPrint(missingIndex);
                p.Link.IsMissing = true;
            }            
        }

        public void Reset()
        {
            this.prints.Clear();
        }

        public void AddPrint(int nistIndex, byte[] wsqPrint)
        {
            this.AddPrint(nistIndex, WsqToBmp.ToBmp(wsqPrint));
        }

        public void AddPrint(int nistIndex, Bitmap img)
        {
            var p = this.GetPrint(nistIndex);            
            p.Reset();

            this.FillModel(p, img);
            
        }

        private Print GetPrint(int index)
        {
            if (!this.prints.ContainsKey(index))
            {
                throw new ApplicationException("Nbis Sequence : Index is not supported : " + index);
            }

            return this.prints[index];
        }        

        private int GetPpi(Bitmap img)
        {
            return (int) img.HorizontalResolution;
        }

        private void FillModel(Print p, Bitmap img)
        {
            if (img == null)
            {
                throw new ApplicationException("Empty Image");
            }

            this.FillPrintInfo(p, img);
            this.ProcessSubPrints(p);            

        }

        private void FillPrintInfo(Print p, Bitmap img)
        {
            try
            {
                p.Image = img;
                p.Minutias = Nbis.DetectMinutiae.FromBitmap(p.Image, this.GetPpi(img));
                p.IsDefined = true;

                p.Quality = Nfiq.FromBitmap(img, this.GetPpi(img));                
            }
            catch (Exception ex)
            {
                p.IsDefined = false;
                p.Image = null;
                p.Minutias = null;

                throw new ApplicationException("Could not Detect minutias of print " + p.Index);
            }
        }

        private void ProcessSubPrints(Print p)
        {
            if (!p.IsDefined || p.SubPrints == null)
            {
                return;
            }
            
            var segPar = new SegmenterParameters
            {
                PermitRotation = true,
                TargetFingers = this.GetFingerIndex(p)
            };

            var seg = Nbis.Segmentation.FromBitmap(p.Image, segPar);

            var allSubPrints = p.SubPrints.Where(x => !x.Link.IsMissing).ToList();
            var currentIndex = 0;

            foreach (var segment in seg)
            {
                var flatSegment = this.GetSubPrint(p.Image, segment);

                if (flatSegment == null)
                {
                    continue;
                }

                if (currentIndex - 1 > allSubPrints.Count )
                {
                    break;
                }

                var subPrint = allSubPrints[currentIndex];
                
                this.FillPrintInfo(subPrint, flatSegment);
                currentIndex += 1;
            }


        }

        private Bitmap GetSubPrint(Bitmap source, Segment segment)
        {
            // segment is too small to contain a print, return null !
            if (segment.Height < 10 || segment.Width < 10)
            {
                return null;
            }

            // Image is too small to contain given segment, return nothing
            if (segment.OriginX + segment.Width - 1 > source.Width ||
                segment.OriginY + segment.Height - 1 > source.Height)
            {
                return null;
            }

            return ImageUtilities.Crop(source, segment.OriginX, segment.OriginY, new Size(segment.Width, segment.Height));

        }

        private Finger GetFingerIndex(Print p)
        {
            switch (p.Link.NistIndex)
            {
                case 13:
                    return Finger.LeftSlap;
                case 14:
                    return Finger.RightSlap;
                case 15:
                    return Finger.BothThumbs;
                default:
                    return Finger.LeftOrRightSlap;
            }           
        }

        public int GetMinutiaCount(int nistIndex)
        {
            var p = this.GetPrint(nistIndex);
            if (!p.IsDefined || p.Minutias == null)
            {
                return 0;
            }
            return p.Minutias.Length;
        }

        public int GetQualityScore(int nistIndex)
        {
            var p = this.GetPrint(nistIndex);
            return p.Quality;
        }

        public int GetMatchScore(int nistIndex)
        {
            var p = this.GetPrint(nistIndex);

            if (!p.IsDefined || !p.Link.GetLinkedPrint(p).IsDefined)
            {
                return -1;
            }

            return Nbis.Matcher.Compare(p.Link.Flat.Minutias, p.Link.Rolled.Minutias);
        }

        public int GetMatchScore(int nistIndex1, int nistIndex2)
        {
            var p1 = this.GetPrint(nistIndex1);
            var p2 = this.GetPrint(nistIndex2);

            if (!p1.IsDefined || !p2.IsDefined)
            {
                return -1;
            }

            return Nbis.Matcher.Compare(p1.Minutias, p2.Minutias);
        }       

        private void GenerateBasePrints()
        {
            for (int i = 1; i < 16; i++)
            {
                var p = new Print {Index = i};
                this.prints.Add(i, p);
            }

            for (int i = 901; i < 910; i++)
            {
                var p = new Print {Index = i, IsSubPrint = true};                
                this.prints.Add(i, p);

                if (i != 906 && i != 901)
                {
                    var rolledIndex = i - 900;
                    var l = new PrintLink {NistIndex = rolledIndex, Flat = p, Rolled = this.prints[rolledIndex]};                
                    this.prints[i].Link = l;
                    this.prints[rolledIndex].Link = l;
                }
                
            }

            this.prints[1].Link = new PrintLink {NistIndex = 1, Flat = this.prints[11], Rolled = this.prints[1]};
            this.prints[11].Link = this.prints[1].Link;

            this.prints[6].Link = new PrintLink { NistIndex = 6, Flat = this.prints[12], Rolled = this.prints[6] };
            this.prints[12].Link = this.prints[6].Link;

            this.prints[13].SubPrints = new List<Print> { this.prints[902], this.prints[903], this.prints[904], this.prints[905] };
            
            this.prints[13].Link = new PrintLink {NistIndex = 13, Flat = this.prints[13]};

            this.prints[14].SubPrints = new List<Print> { this.prints[907], this.prints[908], this.prints[909], this.prints[910] };
            this.prints[14].Link = new PrintLink { NistIndex = 14, Flat = this.prints[14] };

            this.prints[15].SubPrints = new List<Print> { this.prints[11], this.prints[12] };
            this.prints[15].Link = new PrintLink { NistIndex = 15, Flat = this.prints[15] };
            
            this.prints[16].Link = new PrintLink { NistIndex = 16, IsEndorsement = true};
        }

        private class PrintLink
        {

            public Print Rolled { get; set; }
            public Print Flat { get; set; }
            public int NistIndex { get; set; }
            public bool IsMissing { get; set; }
            public bool IsEndorsement { get; set; }

            public Print GetLinkedPrint(Print p)
            {
                return p == this.Rolled ? this.Flat : this.Rolled;
            }
        }

        public void Dispose()
        {
            // nothing yet
        }
    }
}
