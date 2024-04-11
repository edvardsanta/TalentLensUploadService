using System.Text.Json.Serialization;

namespace UploadFiles.Shared.Contracts
{
    public class NormalizeTextMessage
    {
        public string OriginalText {  get; set; } = string.Empty;
    }
}
