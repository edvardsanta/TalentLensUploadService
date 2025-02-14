using MassTransit;
using RankText.Interfaces;
using UploadFiles.Shared.Contracts;

namespace RankText
{
    public class RankTextConsumer : IConsumer<RankTextMessage>
    {
        public ITextClassificationService TextClassificationService { get; }

        public RankTextConsumer(ITextClassificationService textClassificationService)
        {
            TextClassificationService = textClassificationService;
            Task.WaitAll(TextClassificationService.InitializeClassifierAsync());
        }

        public async Task Consume(ConsumeContext<RankTextMessage> context)
        {
            string text = context.Message.NormalizedText;
            string fileId = context.Message.FileId;
            string jobId = context.Message.JobId;

            await TextClassificationService.ClassifyTextAsync(text, fileId, jobId);
        }
    }
}
