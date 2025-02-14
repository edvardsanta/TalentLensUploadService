using UploadFiles.Models;
using UploadFiles.Shared.Enums;
using UploadFiles.Shared.Models.File;

namespace UploadFiles.Services.Mapper
{
    internal static class FileViewerServiceHelpers
    {
        private static float DefineProgress(FileDocument file)
        {
            int totalSteps = Enum.GetValues(typeof(ProcessingStep)).Length;
            int currentJob = (int)file.CurrentJob.CurrentStep;
            float progress = (float)currentJob / totalSteps;
            return progress * 100;
        }

        public static FileViewer MapToViewModel(FileDocument file)
        {
            float progress = DefineProgress(file);

            return new FileViewer
            {
                FileId = file.FileId,
                FileName = file.FileName,
                FileExtension = file.FileExtension.ToString(),
                FileSize = file.FileSize,
                ContentType = file.ContentType,
                CreatedAt = file.CreatedAt,
                UpdatedAt = file.UpdatedAt,
                SkillScores = file.SkillScores,
                Progress = progress,
                Status = file.CurrentJob.Status.ToString(),
                IsError = file.CurrentJob.Status == JobStatus.Failed,
            };
        }
    }
}