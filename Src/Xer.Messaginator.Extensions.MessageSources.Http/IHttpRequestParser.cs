using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Xer.Messaginator.MessageSources.Http
{
    public interface IHttpRequestParser<TMessage>
    {
        /// <summary>
        /// Parse HTTP request to desired message.
        /// </summary>
        /// <param name="httpRequest">HTTP request.</param>
        /// <returns>Message.</returns>
        TMessage Parse(HttpRequest httpRequest);
    }
}