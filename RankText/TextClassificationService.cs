using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using MyML;
using MyML.Interfaces;
using RankText.Interfaces;
using System.Globalization;
using UploadFiles.Infrastructure.Persistence.Repositories;
using UploadFiles.Shared.Enums;

namespace RankText
{
    class TextClassificationService : ITextClassificationService
    {
        public record ResumeData() : ClassifierModel
        {
        }

        private readonly IFileRepository _fileRepository;
        private readonly ILogger<TextClassificationService> _logger;
        private INaiveBayesClassifier _classifier;
        ProcessingStep ProcessingStep = ProcessingStep.SkillScoring;

        public TextClassificationService(
            IFileRepository fileRepository,
            ILogger<TextClassificationService> logger)
        {
            _fileRepository = fileRepository;
            _logger = logger;
            _classifier = new MultinomialNaiveBayesClassifier();
        }

        public async Task InitializeClassifierAsync()
        {
            try
            {
                var trainingData = await LoadTrainingDataAsync();
                _classifier.Train(trainingData);
                _logger.LogInformation("Classifier initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize classifier");
                throw;
            }
        }


        public async Task<Dictionary<string, double>> ClassifyTextAsync(string normalizedText, string fileId, string jobId)
        {
            try
            {
                await _fileRepository.UpdateJobStepAsync(fileId, jobId, ProcessingStep, false);
                var skillScores = _classifier.Predict(normalizedText);

                await _fileRepository.UpdateProcessingResultsAsync(
                    fileId,
                    originalText: null,
                    normalizedText,
                    language: "pt",
                    skillScores,
                    modelVersion: "1.0.0"
                );

                await _fileRepository.UpdateJobStepAsync(fileId, jobId, ProcessingStep, true);

                await _fileRepository.UpdateJobStepAsync(fileId, jobId, ProcessingStep.Complete, true);
                await _fileRepository.UpdateJobStatusAsync(fileId, jobId, JobStatus.Completed);
                return skillScores;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error classifying text for file {FileId}, job {JobId}", fileId, jobId);
                await _fileRepository.UpdateJobStepAsync(fileId, jobId, ProcessingStep, false, ex.Message);
                throw;
            }
        }

        private async Task<List<ResumeData>> LoadTrainingDataAsync()
        {
            const string trainingDataPath = "";
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };

            using var reader = new StreamReader(trainingDataPath);
            using var csv = new CsvReader(reader, config);

            var records = new List<ResumeData>();
            csv.Read();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                var record = csv.GetRecord<ResumeData>();
                if(record != null)
                    records.Add(record);
            }

            return records;
        }
    }
}
