using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using System.Text;
using UploadFiles.Services.Services.Upload.Abstractions;
using UploadFiles.Shared.Enums;
using FileTypeExt = (UploadFiles.Shared.Enums.FileType, UploadFiles.Shared.Enums.FileExtension);

namespace UploadFiles.Services.Services.Upload
{
    public class DocxUpload : DocumentUpload
    {
        public override FileTypeExt FileType { get; set; } = (Shared.Enums.FileType.Document, FileExtension.DOCX);

        public override async Task<string> HandleFileAsync(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Open(stream, true))
                {
                    StringBuilder text = new StringBuilder();
                    foreach (Paragraph para in doc.MainDocumentPart.Document.Body.Elements<Paragraph>())
                    {
                        foreach (Run run in para.Elements<Run>())
                        {
                            text.Append(run.InnerText);
                        }
                        text.AppendLine();
                    }
                }
            }
            return new("");
        }
    }
}