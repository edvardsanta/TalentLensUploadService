using DnsClient.Internal;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TextConsumer.Services.Interfaces;
using UploadFiles.Shared.Contracts;

namespace TextConsumer
{
    public class TextProcessingConsumer : IConsumer<NormalizeTextRecord>
    {
        private readonly IConfiguration _configuration;
        private readonly ITextProcessingService _processingService;
        ILogger<TextProcessingConsumer> _logger;

        public TextProcessingConsumer(IConfiguration configuration, ITextProcessingService processingService, ILogger<TextProcessingConsumer> logger)
        {
            _configuration = configuration;
            _processingService = processingService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<NormalizeTextRecord> context)
        {
            string text = context.Message.OriginalText;
            string fileId = context.Message.FileId;
            string jobId = context.Message.JobId;
            var normalizedText = await _processingService.NormalizeTextAsync(text, fileId, jobId);

            try
            {
                // Publish normalized text for ranking
                await context.Publish(new RankTextMessage(normalizedText, fileId, jobId));

                //// Send to name identifier queue
                //var message = new NameIdentifierMessage { Text = normalizedText };
                //string rabbitMqUri = _configuration["RabbitMq:Uri"];
                //string nameIdentifierQueue = _configuration["RabbitMq:RankTextQueue"];

                //await context.Send(new Uri($"{rabbitMqUri}/{nameIdentifierQueue}"), message);

                _logger.LogInformation("Published normalized text for ranking. FileId: {FileId}",
                    context.Message.FileId);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to publish text for ranking. FileId: {FileId}",
                    context.Message.FileId);
                throw;
            }
        }



        public class NameIdentifierMessage
        {
            public string Text { get; set; }
        }
    }
}
