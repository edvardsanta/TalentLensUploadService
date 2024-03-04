namespace UploadFiles.Services.Services.Upload.Models
{
    public record FileUploadResult(bool SuccessIdentified, bool SuccessSentToProccess, string FileName, 
                                string FileType, string FileExtension,
                                DateTime dateSent)
    {     
    }
}
