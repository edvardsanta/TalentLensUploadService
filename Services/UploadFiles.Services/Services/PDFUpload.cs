using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.AspNetCore.Http;
using System.Text;
using UploadFiles.Services.Services.Abstractions;
using UploadFiles.Services.Utils;

namespace UploadFiles.Services
{
    public class PDFUpload : DocumentUpload
    {
        public override FileType FileType { get; set; } = FileType.Document;

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
