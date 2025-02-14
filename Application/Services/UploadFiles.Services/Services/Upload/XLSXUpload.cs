using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.Text;
using UploadFiles.Services.Services.Upload.Abstractions;
using UploadFiles.Shared.Enums;
using FileTypeExt = (UploadFiles.Shared.Enums.FileType, UploadFiles.Shared.Enums.FileExtension);


namespace UploadFiles.Services.Services.Upload
{
    public class XLSXUpload : DocumentUpload
    {
        public override FileTypeExt FileType { get; set; } = (Shared.Enums.FileType.Document, FileExtension.XLSX);

        /// <summary>
        /// Handles uploading an Excel (.xlsx) file by reading the contents into a string
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <returns>A task</returns>  
        public async override Task<string> HandleFileAsync(IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            stream.Position = 0;

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var text = new StringBuilder();
                for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
                {
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        text.Append(worksheet.Cells[row, col].Value?.ToString() ?? "");
                    }
                }
                string result = text.ToString();
            }

            return new("");
        }
    }
}
