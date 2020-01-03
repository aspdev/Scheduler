﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Text;

namespace Client.Torun.RavenDataService.Extentions
{
    public static class HttpResponseExtention
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer()
        {
            NullValueHandling = NullValueHandling.Ignore,

        };


        public static void WriteJson<T>(this HttpResponse response, T obj)
        {
            response.ContentType = "application/json";

            using (var writer = new HttpResponseStreamWriter(response.Body, Encoding.UTF8))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    jsonWriter.CloseOutput = false;
                    jsonWriter.AutoCompleteOnClose = false;

                    Serializer.Serialize(jsonWriter, obj);

                }
            }
        }
    }
}
