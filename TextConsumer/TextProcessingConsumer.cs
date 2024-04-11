using MassTransit;
using Newtonsoft.Json;
using TextProcessing;
using UploadFiles.Shared.Contracts;

namespace TextConsumer
{
    public class TextProcessingConsumer : IConsumer<NormalizeTextMessage>
    {
        public TextProcessingConsumer()
        {

        }

        public async Task Consume(ConsumeContext<NormalizeTextMessage> context)
        {
            string text = context.Message.OriginalText;

            string normalizedText = await new Processing().Process(text, "pt", true);

            await context.Publish(new RankTextMessage(normalizedText));

            var message = new NameIdentifierMessage { Text = normalizedText };

            // TODO : REMOVE MOCK URI
            await context.Send(new Uri("rabbitmq://rabbitmq/name_identifier_queue"), message);

            Console.WriteLine("Message published: " + message.Text);
        }
        public class NameIdentifierMessage
        {
            public string Text { get; set; }
        }
    }
}
