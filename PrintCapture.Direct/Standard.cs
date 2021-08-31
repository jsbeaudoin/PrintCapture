using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintsCapture.Direct
{
    using UniBIO.Services.Communication.BiometricService;

    public class Standard
    {
        public CapturedPrintData Capture()
        {
            try
            {
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
