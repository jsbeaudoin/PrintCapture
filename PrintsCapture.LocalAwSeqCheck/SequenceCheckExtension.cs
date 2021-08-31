using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aware.AwSequence;
using NLog;
using XL_ID.Utilities.Log;

namespace PrintsCapture.LocalAwSeqCheck
{
    public static class SequenceCheckExtension
    {

        public static int GetQualityScore(this awSequenceCheck seq, awSequenceCheck.AwareFingerType fingerType)
        {
            int quality = 0;

            try
            {
                quality = seq.NFIQFingerScore(fingerType);
            }
            catch (Exception ex)
            {
                LogDispatcher.DoLog("NFIQFingerScore Failed", LogEventLevel.Warning, ex);                
            }

            if (quality == 0)
            {
                try
                {
                    quality = 6 - (int)Math.Ceiling(seq.QualityFingerScore(fingerType) / 20F);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("QualityFingerScore Failed : {0}", ex);                    
                }
            }

            return quality;
        }

    }
}
