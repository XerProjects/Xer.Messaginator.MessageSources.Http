using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Xer.Messaginator.MessageSources.Http
{
    public class HttpRequestJsonParser<TMessage> : IHttpRequestParser<TMessage>
    {
        private static readonly JsonSerializer _serializer = new JsonSerializer()
        {
            Formatting = Formatting.Indented
        };

        /// <summary>
        /// Parse HTTP request to desired message.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Message.</returns>
        public TMessage Parse(HttpRequest httpRequest)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            using (StreamReader reader = new StreamReader(httpRequest.Body))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                return _serializer.Deserialize<TMessage>(jsonReader);
            }
        }
    }
}