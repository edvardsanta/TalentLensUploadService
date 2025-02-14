using Microsoft.AspNetCore.Http;
using System.Collections.Immutable;
using UploadFiles.Shared.Enums;
using FileTypeExt = (UploadFiles.Shared.Enums.FileType type, UploadFiles.Shared.Enums.FileExtension ext);
namespace UploadFiles.Services.Mapper
{
    public class FileTypeMapper
    {
        private static readonly ImmutableDictionary<string, FileType> ExtensionToTypeMap =
            new Dictionary<string, FileType>
            {
                [".doc"] = FileType.Document,
                [".docx"] = FileType.Document,
                [".pdf"] = FileType.Document,
                [".jpg"] = FileType.Image,
                [".jpeg"] = FileType.Image,
                [".png"] = FileType.Image
            }.ToImmutableDictionary();

        public FileTypeExt DetermineFileType(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileType = ExtensionToTypeMap.GetValueOrDefault(extension, FileType.Unknown);
            var fileExtension = ParseFileExtension(extension);

            return (fileType, fileExtension);
        }

        private static FileExtension ParseFileExtension(string extension)
        {
            var cleanExtension = extension.TrimStart('.').ToUpperInvariant();
            return Enum.TryParse(cleanExtension, true, out FileExtension result)
                ? result
                : FileExtension.Unknown;
        }
    }
}
