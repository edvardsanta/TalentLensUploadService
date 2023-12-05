using System.Collections.Immutable;
using UploadFiles.Interfaces;
using UploadFiles.Services;

namespace UploadFiles
{
    public static class Configuration
    {
        public static ImmutableList<IFileHandler> ConfigureFileHandlers()
        {
            return [ new PDFUpload() ];
        }
    }
}
