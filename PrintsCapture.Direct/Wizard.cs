using System;
using System.Collections.Generic;

namespace PrintsCapture.Direct
{
    using System.Threading;
    using UniBIO.Services.Communication.BiometricService;

    public class Wizard: IDisposable
    {        
        public CapturedPrintData Capture(Dictionary<string, string> arguments)
        {
            var t = new DomainPrintCapture();
            var result =  t.CaptureData(arguments);
            
            t.Dispose();
            return result;
        }

        public void Dispose()
        {
            GC.Collect();
        }
    }
}
