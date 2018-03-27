using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Xer.Messaginator.MessageSources.Http
{
    public class HttpMessageSource<TMessage> : IMessageSource<TMessage>, 
                                               IDisposable 
                                               where TMessage : class
    {
        #region Declarations

        private readonly IHttpRequestParser<TMessage> _httpRequestParser;
        private readonly Action<IRouteBuilder> _routeBuilder;
        private readonly Action<IServiceCollection> _servicesBuilder;
        private readonly Action<ILoggingBuilder> _loggingBuilder;
        private CancellationToken _receiveCancellationToken;
        private IWebHost _host;

        #endregion Declarations
        
        #region Properties

        /// <summary>
        /// Url which is hosts this message source.
        /// </summary>
        public string Url { get; }

        #endregion Properties

        #region Events
        
        /// <summary>
        /// Errors are published through this event.
        /// </summary>
        public event EventHandler<Exception> OnError;

        /// <summary>
        /// Received messages are published through this event. 
        /// </summary>
        public event MessageReceivedDelegate<TMessage> OnMessageReceived;

        #endregion Events

        #region Constructor
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="url">Url which is hosts this message source.</param>
        /// <param name="httpRequestParser">HTTP request parser.</param>
        /// <param name="routeBuilder">Route builder.</param>
        /// <param name="servicesBuilder">Services builder.</param>
        /// <param name="loggingBuilder">Logging builder.</param>
        internal HttpMessageSource(string url,
                                   IHttpRequestParser<TMessage> httpRequestParser = null,
                                   Action<IRouteBuilder> routeBuilder = null, 
                                   Action<IServiceCollection> servicesBuilder = null,
                                   Action<ILoggingBuilder> loggingBuilder = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("Url is required.", nameof(url));
            }

            Url = url;
            _httpRequestParser = httpRequestParser ?? new HttpRequestJsonParser<TMessage>();
            _loggingBuilder = loggingBuilder ?? ConfigureDefaultLogging;
            _routeBuilder = routeBuilder ?? ConfigureDefaultRouting;
            _servicesBuilder = servicesBuilder ?? ConfigureDefaultServices;
        }

        #endregion Constructor

        #region IMessageSource<TMessage> Implementation

        /// <summary>
        /// Receive messages via in-process.
        /// </summary>
        /// <param name="message">Message to receive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        public Task ReceiveAsync(MessageContainer<TMessage> message, CancellationToken cancellationToken = default(CancellationToken))
        {
            PublishMessage(message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Start receving messages via HTTP.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        public Task StartReceivingAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _receiveCancellationToken = cancellationToken;
            
            IWebHostBuilder webHostBuilder = new WebHostBuilder()
                .ConfigureServices(_servicesBuilder)
                .ConfigureLogging(_loggingBuilder)
                .Configure(app => app.UseRouter(_routeBuilder))
                .UseUrls(Url);

            _host = ConfigureWebHost(webHostBuilder);

            if (_host == null)
            {
                return Task.FromException(new InvalidOperationException("No web host was configured."));
            }
 
            return _host.StartAsync(cancellationToken);
        }

        /// <summary>
        /// Stop receving messages via HTTP.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        public Task StopReceivingAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _host?.StopAsync(cancellationToken);
        }

        #endregion IMessageSource<TMessage> Implementation

        #region Methods

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            _host?.Dispose();
        }     

        /// <summary>
        /// Configure web host.
        /// </summary>
        /// <param name="webHostBuilder">Web host builder.</param>
        /// <returns>Web host.</returns>
        protected virtual IWebHost ConfigureWebHost(IWebHostBuilder webHostBuilder)
        {
            return webHostBuilder.UseKestrel().Build();
        }

        /// <summary>
        /// Handle HTTP request.
        /// </summary>
        /// <param name="context">HTTP context.</param>
        /// <returns>Asynchronous task that can be awaited for completion.</returns>
        protected virtual Task HandleHttpRequestAsync(HttpContext context)
        {
            try
            {
                // Deserialize HTTP request body.
                TMessage receivedMessage = _httpRequestParser.Parse(context.Request);

                // Publish message.
                PublishMessage(new MessageContainer<TMessage>(receivedMessage));
            }
            catch (Exception ex)
            {
                PublishException(ex);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Publish received message through <see cref="OnMessageReceived"/>.
        /// </summary>
        /// <param name="receivedMessage">Received message.</param>
        protected void PublishMessage(MessageContainer<TMessage> receivedMessage)
        {
            if (receivedMessage != null && !receivedMessage.IsEmpty)
            {
                OnMessageReceived?.Invoke(receivedMessage);
            }
        }

        /// <summary>
        /// Publish exception through <see cref="OnError"/>.
        /// </summary>
        /// <param name="exception">Exception.</param>
        protected void PublishException(Exception exception)
        {
            if (exception != null)
            {
                OnError?.Invoke(this, exception);
            }
        }

        #endregion Methods

        #region Functions
        
        /// <summary>
        /// Configure default logging.
        /// </summary>
        /// <remarks>Default implementation adds console routing.</remarks>
        /// <param name="loggingBuilder">Logging builder.</param>
        private void ConfigureDefaultLogging(ILoggingBuilder loggingBuilder) => loggingBuilder.AddConsole();

        /// <summary>
        /// Configure default services.
        /// </summary>
        /// <remarks>Default implementation adds routing services.</remarks>
        /// <param name="services">Service collection.</param>
        private void ConfigureDefaultServices(IServiceCollection services) => services.AddRouting();
        
        /// <summary>
        /// Configure default request routing.
        /// </summary>
        /// <remarks>Default implementation routes all request to the root.</remarks>
        /// <param name="routeBuilder">Route builder.</param>
        private void ConfigureDefaultRouting(IRouteBuilder routeBuilder) => routeBuilder.MapPost(string.Empty, HandleHttpRequestAsync);

        #endregion Functions
    }
}
