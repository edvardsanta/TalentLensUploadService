using MassTransit;
using UploadFiles.Shared.Contracts;

namespace TextConsumer
{
    public class TextProcessingConsumer : IConsumer<RankTextMessage>
    {
        public TextProcessingConsumer()
        {

        }

        public async Task Consume(ConsumeContext<RankTextMessage> context)
        {
            var text = context.Message.extractedText;

            var normalizedText = ProcessText(text);

            //await context.Publish(new RankTextMessage(""));
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
