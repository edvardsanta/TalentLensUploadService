namespace RankText.Interfaces
{
    public interface ITextClassificationService
    {
        Task<Dictionary<string, double>> ClassifyTextAsync(string normalizedText, string fileId, string jobId);
        Task InitializeClassifierAsync();
    }
}
