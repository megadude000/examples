using Company.Pebbles.Configuration.Configuration;
using Company.Pebbles.Core.Services.Interfaces;
using Company.Pebbles.Examination.Enumerations;
using Company.Pebbles.Examination.Models;
using Company.Pebbles.Examination.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Pebbles.Examination.Services
{
    internal class ReportService : IReportService
    {
        private readonly IList<ExaminationReportModel> reports = new List<ExaminationReportModel>();

        private readonly IDateTimeService dateTimeService;
        private readonly IExaminationInformation examinationInformation;
        private readonly IExaminationRemoteSettings examinationRemoteSettings;
        private readonly IHttpRequestService examinationHttpRequestService;

        public ReportService(IDateTimeService dateTimeService,
            IExaminationInformation examinationInformation,
            IExaminationRemoteSettings examinationRemoteSettings,
            IHttpRequestService examinationHttpRequestService)
        {
            this.dateTimeService = dateTimeService;
            this.examinationInformation = examinationInformation;
            this.examinationRemoteSettings = examinationRemoteSettings;
            this.examinationHttpRequestService = examinationHttpRequestService;
        }

        public async Task SaveReportsToServer()
        {
            if (reports.Count == 0)
                return;

            var summary = GetExaminationSummary();
            var reportInformation = new ExaminationReportsInformationModel
            {
                TimeUps = summary.TimeUps,
                ExaminationReports = reports.ToArray(),
                AllQuestions = reports.Count(),
                CorrectAnswers = summary.CorrectAnswers,
                FailedAnswers = summary.IncorrectAnswers,
                AverageReactionTime = summary.AverageReactionTime,
                EndDate = dateTimeService.GetCurrentServerDateTime(),
                InstanceId = examinationRemoteSettings.InstanceId,
                UserInformation = examinationInformation.User,
                AllAnswers = summary.IncorrectAnswers + summary.CorrectAnswers,
                StartDate = examinationInformation.State.StartTime,
                ExaminationId = examinationInformation.State.CurrentExaminationId,
            };

            if (summary.AnsweredTestCasesCount != 0 || summary.TimeUps != 0)
            {
                await examinationHttpRequestService.SaveExaminationReportAsync(reportInformation);
            }

            reports.Clear();
        }

        public void AddReport(string testCaseName, TimeSpan spentTime, ExaminationAnswerModel answer, AnswerStatus answerStatus, long questionId)
        {
            var report = new ExaminationReportModel
            {
                Answer = answer,
                Status = answerStatus,
                SpentTime = spentTime,
                QuestionId = questionId,
                TestCaseName = testCaseName
            };
            reports.Add(report);
        }

        public ExaminationSummaryModel GetExaminationSummary()
        {
            return new ExaminationSummaryModel
            {
                AverageReactionTime = GetAverageReactionTime(),
                CorrectnessPercentage = GetPercentOfCorrectness(),
                TimeUps = reports.Where(item => item.Status == AnswerStatus.TimeUp).Count(),
                CorrectAnswers = reports.Where(item => item.Status == AnswerStatus.Correct).Count(),
                IncorrectAnswers = reports.Where(item => item.Status == AnswerStatus.Incorrect).Count(),
                AnsweredTestCasesCount = reports.Where(item => item.Status != AnswerStatus.TimeUp && item.Status != AnswerStatus.Stopped).Count()
            };
        }

        public bool IsReportExist(string fileName)
        {
            return reports.Any(item => item.TestCaseName == fileName);
        }

        private double GetAverageReactionTime()
        {
            var selectedValues = reports.Where(report => report.Status != AnswerStatus.Stopped).Select(item => item.SpentTime.TotalSeconds);
            var avrg = selectedValues.Any() ? selectedValues.Average() : 0;
            return Math.Round(avrg, 2);
        }

        private double GetPercentOfCorrectness()
        {
            if (reports.Any())
            {
                var correctAnswersCount = reports.Where(report => report.Status == AnswerStatus.Correct).Count();
                double percent = correctAnswersCount / ((double)reports.Count / 100);
                return Math.Round(percent, 2);
            }
            else
                return 0;
        }
    }
}
