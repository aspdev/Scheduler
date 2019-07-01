using System;
using System.Collections.Generic;
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
                dataToReturn.Add(kv.Key.ToShortDateString(), kv.Value);
            }

            return dataToReturn;

        }


    }
}
