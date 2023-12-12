using MassTransit;
using UploadFiles.Shared.Contracts;

namespace TextConsumer
{
    public class TextProcessingConsumer : IConsumer<TextMessage>
    {
        public async Task Consume(ConsumeContext<TextMessage> context)
        {
            var text = context.Message.Text;

            var normalizedText = ProcessText(text);

            await Task.Delay(1000);
        }

        private string ProcessText(string text)
        {
            return text;
        }

        private string RemoveStopWords(string text)
        {
            return text;
        }
    }
}
