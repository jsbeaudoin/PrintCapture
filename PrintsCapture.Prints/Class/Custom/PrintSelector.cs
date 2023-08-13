using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintsCapture.Prints.Class.Custom
{
    public class PrintSelector
    {
		public String Key { get; set; }

		public bool IsEndorsement { get { return this.Key == "E"; } }

		public int NistIndex {  get {
                int valueNumber = 0;
                int.TryParse(this.Key, out valueNumber);
                return valueNumber;
            }
        }
    }
}
