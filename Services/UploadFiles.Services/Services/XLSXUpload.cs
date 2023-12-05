using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UploadFiles.Services.Services.Abstractions;
using UploadFiles.Services.Utils;

namespace UploadFiles.Services.Services
{
    internal class XLSXUpload : DocumentUpload
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
