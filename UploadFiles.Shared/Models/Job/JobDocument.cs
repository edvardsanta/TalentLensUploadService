using UploadFiles.Shared.Enums;
namespace UploadFiles.Shared.Models.Job
{
    public class JobDocument
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public JobStatus Status { get; set; } = JobStatus.Created;
        public ProcessingStep CurrentStep { get; set; }
        public Dictionary<string, StepStatus> Steps { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string ErrorMessage { get; set; }
        public string ModelVersion { get; set; }
        public Dictionary<string, object> JobMetadata { get; set; } = new();


        public void InitializeSteps()
        {
            foreach (var step in Enum.GetValues(typeof(ProcessingStep)))
            {
                Steps[step.ToString()] = new StepStatus
                {
                    Step = step.ToString(),
                    IsCompleted = false
                };
            }
        }

        public bool UpdateStepStatus(ProcessingStep step, bool isCompleted, string? errorMessage = null)
        {
            string stepName = step.ToString();
            if (!Steps.ContainsKey(stepName))
                return false;

            var stepStatus = Steps[stepName];

            if (!stepStatus.StartedAt.HasValue)
            {
                stepStatus.StartedAt = DateTime.UtcNow;
                CurrentStep = step;
            }

            if (isCompleted)
            {
                stepStatus.IsCompleted = true;
                stepStatus.CompletedAt = DateTime.UtcNow;

                var nextStep = GetNextStep();
                if (nextStep.HasValue)
                    CurrentStep = nextStep.Value;
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                stepStatus.ErrorMessage = errorMessage;
                ErrorMessage = errorMessage;
            }

            return true;
        }

        public ProcessingStep? GetNextStep()
        {
            var steps = Enum.GetValues(typeof(ProcessingStep))
                .Cast<ProcessingStep>()
                .OrderBy(s => s);

            foreach (var step in steps)
            {
                if (!Steps[step.ToString()].IsCompleted)
                    return step;
            }
            return null;
        }

    }

    public class StepStatus
    {
        public string Step { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, object> StepMetadata { get; set; } = new();
    }
}
