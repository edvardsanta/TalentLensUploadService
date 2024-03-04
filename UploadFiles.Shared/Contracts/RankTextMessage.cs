using System.Text.Json.Serialization;

namespace UploadFiles.Shared.Contracts
{
    [JsonSerializableAttribute(typeof(RankTextMessage))]
    public class RankTextMessage
    {
        public string extractedText {  get; set; } = string.Empty;
    }
}
