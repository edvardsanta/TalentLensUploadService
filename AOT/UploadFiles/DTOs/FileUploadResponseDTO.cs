namespace UploadFiles.DTOs
{
    public class FileUploadResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ReferenceId { get; set; }
        public string EstimatedProcessingTime { get; set; }
        public string StatusCheckUrl { get; set; }
        public string NextSteps { get; set; }

        public FileUploadResponseDTO(bool success, string message, string referenceId, string estimatedProcessingTime, string statusCheckUrl, string nextSteps, string supportContact)
        {
            Success = success;
            Message = message;
            ReferenceId = referenceId;
            EstimatedProcessingTime = estimatedProcessingTime;
            StatusCheckUrl = statusCheckUrl;
            NextSteps = nextSteps;
        }
    }
}
