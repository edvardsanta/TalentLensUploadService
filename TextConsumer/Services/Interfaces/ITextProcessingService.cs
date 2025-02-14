namespace TextConsumer.Services.Interfaces
{
    public interface ITextProcessingService
    {
        Task<string> NormalizeTextAsync(string originalText, string fileId, string jobId);
    }
}
