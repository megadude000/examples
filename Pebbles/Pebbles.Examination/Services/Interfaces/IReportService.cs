using System;
using Company.Pebbles.Examination.Models;
using System.Threading.Tasks;
using Company.Pebbles.Examination.Enumerations;

namespace Company.Pebbles.Examination.Services.Interfaces
{
    public interface IReportService
    {
        Task SaveReportsToServer();
        bool IsReportExist(string fileName);
        ExaminationSummaryModel GetExaminationSummary();
        void AddReport(string testCaseName, TimeSpan spentTime, ExaminationAnswerModel answer, AnswerStatus answerStatus, long questionId);
    }
}
