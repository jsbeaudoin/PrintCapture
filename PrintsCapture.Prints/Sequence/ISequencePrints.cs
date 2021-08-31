using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintsCapture.Prints.Sequence
{
    public interface ISequencePrints
    {
        List<PrintInfo> GetCapturedPrints();

        List<PrintInfo> GetCapturedSegments();

    }
}
