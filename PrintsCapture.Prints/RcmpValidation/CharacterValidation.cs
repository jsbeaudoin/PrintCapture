using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintsCapture.Prints.RcmpValidation
{
    [Flags]
    public enum ValidationKind
    {
        Undefined,

        /// <summary>
        /// refers to all upper case letters (A-Z) defined by ASCII character set from 65 to 90 inclusive.
        /// </summary>
        Alpha=1,

        /// <summary>
        /// refers to 10 digits (0-9) defined by ASCII character set from 48 to 57
        /// </summary>
        Numeric=2,

        /// <summary>
        /// # $ & ’( ) * , -. defined by ASCII character set from 35 to 46 AND 32 (Space )
        /// </summary>
        SpecialChar=4,

        /// <summary>
        /// @ ASCII 64
        /// </summary>
        ArobasAllowed=32,

        /// <summary>
        /// Cr Lf ASCII --> 13,10
        /// </summary>
        CrLfAllowed=64
    }

    public class CharacterValidation
    {
    }
}
