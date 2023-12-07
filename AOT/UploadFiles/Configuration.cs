using System.Collections.Immutable;
using UploadFiles.Interfaces;
using UploadFiles.Services;
using UploadFiles.Services.Services;

namespace UploadFiles
{
    public static class Configuration
    {
        public static ImmutableList<IFileHandler> ConfigureFileHandlers() => [
                new PDFUpload(),
                new XLSXUpload()
                ];
    }
}
