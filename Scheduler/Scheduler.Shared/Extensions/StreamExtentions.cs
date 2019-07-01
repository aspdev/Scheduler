using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Scheduler.Shared.Extensions
{
    public static class StreamExtentions
    {
        // T to typ obiektu do którego chcemy Deseriazlizować Json --> Stream

        public static T ReadAndDeserializeFromJson<T>(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new NotSupportedException("Can't read from this stream");
            }

            using (var streamReader = new StreamReader(stream))
            {
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var jsonSerializer = new JsonSerializer();
                    return jsonSerializer.Deserialize<T>(jsonTextReader);
                }
            }
        }

        public static async Task<string> ToStringAsync(this Stream stream)
        {
            string content = null;

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new NotSupportedException("Can't read from this stream");
            }

            using (var streamReader = new StreamReader(stream))
            {
                content = await streamReader.ReadToEndAsync();
            }

            return content;

        }

        
    }
}
