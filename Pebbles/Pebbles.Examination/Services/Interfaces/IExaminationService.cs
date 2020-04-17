using System;
using Company.Pebbles.Examination.Models;
using System.Threading.Tasks;
using Company.Pebbles.Examination.Enumerations;

namespace Company.Pebbles.Examination.Services.Interfaces
{
    public interface IExaminationService
    {
        event Action ExaminationStopped;
        int TestCasesCount { get; }
        bool IsNextTestCaseExist { get; }

        Task StopExamination();
        Task StartExamination(long examinationId);
        void StopAudioIfPlayed();
        void MoveToNextTextCase(float volume);
        bool IsNextAnswerNeeded(ExaminationAnswerModel selectedAnswer);
        ExaminationSummaryModel GetExaminationSummary();
        AnswerStatus StopTestCase(ExaminationAnswerModel answer);
    }
}
