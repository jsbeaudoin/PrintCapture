using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintsCapture.Ui.Extension
{
    public static class EnumFlagExtension
    {


        public static List<Enum> GetValues(this Enum flagEnum, bool exludeValue0 = true)
        {

            var result = new List<Enum>();

            var allValues = Enum.GetValues(flagEnum.GetType());

            foreach (var value in allValues)
            {
                var numValue = (int)value;
                var val = value as Enum;
                if (val != null && flagEnum.HasFlag(val) && (numValue != 0 || !exludeValue0 ))
                {
                    result.Add(val);
                }
            }

            return result;
        }

    }
}
