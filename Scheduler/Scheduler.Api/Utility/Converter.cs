using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Scheduler.Api.Utility
{
    public class Converter
    {
        public static Dictionary<string, object> ConvertDateTimeToString(Dictionary<DateTime, object> data)
        {
            Dictionary<string, object> dataToReturn = new Dictionary<string, object>();

            foreach (var kv in data)
            {
                string date = kv.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                dataToReturn.Add(date, kv.Value);
            }

            return dataToReturn;

        }


    }
}
