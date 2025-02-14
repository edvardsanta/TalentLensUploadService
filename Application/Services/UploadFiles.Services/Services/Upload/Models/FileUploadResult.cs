namespace UploadFiles.Services.Services.Upload.Models
{
    public record FileUploadResult(bool SuccessIdentified, string FileName,
                                string extractedText,
                                string FileType, string FileExtension,
                                DateTime dateSent)
    {
        public string FileId { get; set; } = string.Empty;
        public string JobId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}
