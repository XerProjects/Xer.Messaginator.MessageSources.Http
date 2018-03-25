using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xer.Messaginator;

namespace ConsoleApp.HttpMessageSource.Entities
{
    public class SampleMessageProcessor : MessageProcessor<SampleMessage>
    {
        public override string Name => "SampleMessageProcessor";

        public SampleMessageProcessor(IMessageSource<SampleMessage> messageSource)
            : base(messageSource)
        {
        }

        protected override Task ProcessMessageAsync(MessageContainer<SampleMessage> receivedMessage, CancellationToken cancellationToken)
        {
            // Implicit conversion.
            SampleMessage message = receivedMessage;

            System.Console.WriteLine("----------------------------------------------------------------------------------------------");
            System.Console.WriteLine($"Message processed by {GetType().Name}: | Id=[{message.Id}] | Message=[{message.Message}] |");
            System.Console.WriteLine("----------------------------------------------------------------------------------------------");

            return Task.CompletedTask;
        }
    }
}