using PrintsCapture.Prints;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XL_ID.Utilities.Log;

namespace PrintsCapture.Prints.Class.Custom
{
    public class CustomPrintOrderManager
    {
        const String OrderFileName = "OrderConfig.xlid";
        private String FilePath { get; }

        public bool IsValid { get; private set; }

        /// <summary>
        /// Returns a value indicating if the custom order can be used, if the capture rules allows it.
        /// </summary>
        public bool CanBeUsed { get; private set; }

        public CustomPrintOrderManager(PrintRules rules)
        {
            this.FilePath = Path.Combine(PrintAppPath.AppPath, OrderFileName);
            this.IsValid = false;
            this.CanBeUsed = (rules.CaptureKind == Enum.CaptureKind.Livescan && rules.OrderMode != Enum.CaptureOrderMode.Sq && rules.CaptureGroup != Enum.PrintCaptureGroup.FlatOnly);
            this.load();
        }

        public bool IsCustomFilePresent
        {
            get
            {
                return (File.Exists(this.FilePath));
            }
        }

        public bool IsCustomFileValid()
        {
            return this.IsValid;
        }

        public List<PrintSelector> PrintList { get; private set; }

        private void load()
        {
            this.IsValid = false;
            if (! IsCustomFilePresent)
            {
                return;
            }

            try
            {
                this.PrintList = XL_ID.Utilities.XML.ObjectSerializer.GetInstanceFromXml<List<PrintSelector>>(this.FilePath);
            }
            catch (Exception ex)
            {
                LogDispatcher.DoLog("Could not load custom order file", LogEventLevel.Warning, ex);
                return;
            }

            if (this.PrintList == null)
            {
                this.IsValid = false;
                LogDispatcher.DoLog("Could not load custom order file, file is invalid", LogEventLevel.Warning);

            } else
            {
                this.IsValid = true;
            }

        }

        public void SaveTest()
        {
            if (this.IsCustomFilePresent) // dont overwrite existing file with test values
            {
                return;
            }
            // saves a test file with all values
            List<PrintSelector> testList = new List<PrintSelector>();
            List<String> keys = new List<String> { "15", "13", "14", "1", "2", "3", "4", "5", "26", "25", "22", "14", "6", "7", "8", "9", "10", "28", "27", "24", "E" };

            foreach (var key in keys)
            {
                testList.Add(new PrintSelector()
                {
                    Key = key
                });
            }

            XL_ID.Utilities.XML.ObjectSerializer.SaveInstanceToXml(this.FilePath, testList);
        }

    }
}
