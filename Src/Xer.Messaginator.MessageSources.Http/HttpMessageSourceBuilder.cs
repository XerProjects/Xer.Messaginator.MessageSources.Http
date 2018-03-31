using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Xer.Messaginator.MessageSources.Http
{
    public class HttpMessageSourceBuilder<TMessage> where TMessage : class
    {
        private string _url;
        private IHttpRequestParser<TMessage> _httpRequestParser = new HttpRequestJsonParser<TMessage>();
        private Action<IRouteBuilder> _routeBuilder;
        private Action<IServiceCollection> _servicesBuilder;
        private Action<ILoggingBuilder> _loggingBuilder;

        /// <summary>
        /// Make HttpMessageSource listen to the specified URL.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <returns>HttpMessageSourceBuilder.</returns>
        public HttpMessageSourceBuilder<TMessage> ListenInUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL cannot be empty or whitespace.", nameof(url));
            }

            _url = url;
            return this;
        }

        /// <summary>
        /// Use HTTP request parser.
        /// </summary>
        /// <param name="httpRequestParser">HTTP request parser.</param>
        /// <returns>HttpMessageSourceBuilder.</returns>
        public HttpMessageSourceBuilder<TMessage> UseHttpRequestParser(IHttpRequestParser<TMessage> httpRequestParser)
        {
            _httpRequestParser = httpRequestParser ?? throw new ArgumentNullException(nameof(httpRequestParser));
            return this;
        }

        /// <summary>
        /// Configure logging.
        /// </summary>
        /// <param name="loggingBuilder">Action to configure logging.</param>
        /// <returns>HttpMessageSourceBuilder.</returns>
        public HttpMessageSourceBuilder<TMessage> ConfigureLogging(Action<ILoggingBuilder> loggingBuilder)
        {
            _loggingBuilder = loggingBuilder ?? throw new ArgumentNullException(nameof(loggingBuilder));
            return this;
        }

        /// <summary>
        /// Configure routing.
        /// </summary>
        /// <param name="routeBuilder">Action to configure routing.</param>
        /// <returns>HttpMessageSourceBuilder.</returns>
        public HttpMessageSourceBuilder<TMessage> ConfigureRouting(Action<IRouteBuilder> routeBuilder)
        {
            _routeBuilder = routeBuilder ?? throw new ArgumentNullException(nameof(routeBuilder));
            return this;
        }

        /// <summary>
        /// Configure services.
        /// </summary>
        /// <param name="routeBuilder">Action to configure services.</param>
        /// <returns>HttpMessageSourceBuilder.</returns>
        public HttpMessageSourceBuilder<TMessage> ConfigureServices(Action<IServiceCollection> servicesBuilder)
        {
            _servicesBuilder = servicesBuilder ?? throw new ArgumentNullException(nameof(servicesBuilder));
            return this;
        }

        /// <summary>
        /// Build MessageSource.
        /// </summary>
        /// <returns>Instance of MessageSource.</returns>
        public HttpMessageSource<TMessage> Build()
        {
            if (string.IsNullOrWhiteSpace(_url))
            {
                throw new InvalidOperationException("Cannot build HTTP message source. Listening URL must be specified.");
            }

            return new HttpMessageSource<TMessage>(_url, _httpRequestParser, _routeBuilder, _servicesBuilder, _loggingBuilder);
        }
    }
}