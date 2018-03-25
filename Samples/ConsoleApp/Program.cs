using System;
using System.Net.Http;
using System.Threading.Tasks;
using ConsoleApp.HttpMessageSource.Entities;
using Newtonsoft.Json;
using Xer.Messaginator;
using Xer.Messaginator.MessageSources.Http;

namespace ConsoleApp.HttpMessageSource
{
    class Program
    {
        private static readonly string _messageSourceUrl = "http://localhost:6007";
        private static readonly HttpClient _httpClient = new HttpClient();
        static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        static async Task MainAsync(string[] args)
        {
            // This message source will open a HTTP port to receive POST requests.
            HttpMessageSource<SampleMessage> httpMessageSource = new HttpMessageSourceBuilder<SampleMessage>()
                .ListenInUrl(_messageSourceUrl)
                .UseHttpRequestParser(new HttpRequestJsonParser<SampleMessage>())
                .Build();

            // This message processor will process messages received and published by the message source.
            MessageProcessor<SampleMessage> messageProcessor = new SampleMessageProcessor(httpMessageSource);

            Console.WriteLine("Press any key to start message processing.");
            Console.ReadLine();

            Console.WriteLine("Starting...");
            // Will not block.
            await messageProcessor.StartAsync();

            while (true)
            {
                // Enter raw text to send to the message source.
                Console.WriteLine($"Enter message to send to HTTP message source in {_messageSourceUrl}:");
                string input = Console.ReadLine();

                // Do nothing if input is empty.
                if (string.IsNullOrEmpty(input)) continue;

                // Stop if triggered.
                if (string.Equals(input, "stop", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Stopping...");
                    // Wait til last message is finished.
                    await messageProcessor.StopAsync();
                    break;
                }
                
                // Create message.
                var message = new SampleMessage() { Message = input };

                Console.WriteLine("-------------------------------------------------------------------------------------------------------------");
                Console.WriteLine($"Sending message to HTTP message source for processing: | Id=[{message.Id}] | Message=[{message.Message}] |");
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------");
                
                // Send message to the HttpMessageSource.
                await _httpClient.PostAsync(_messageSourceUrl, new StringContent(JsonConvert.SerializeObject(message)));
            }
        }
    }
}
