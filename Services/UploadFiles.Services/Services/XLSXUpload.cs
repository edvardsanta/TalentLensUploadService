using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.AspNetCore.Http;
using System.Text;
using UploadFiles.Services.Services.Abstractions;
using UploadFiles.Services.Utils;
using FileTypeExt = (UploadFiles.Services.Utils.FileType, UploadFiles.Services.Utils.FileExtension);

namespace UploadFiles.Services.Services
{
    public class XLSXUpload : DocumentUpload
    {
        public override FileTypeExt FileType { get; set; } = (Utils.FileType.Document, FileExtension.XLSX);

        public async override Task HandleFileAsync(IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            stream.Position = 0;

            using var reader = new PdfReader(stream);

            StringBuilder text = new StringBuilder();

            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
            }
            string result = text.ToString();
        }
    }
}
