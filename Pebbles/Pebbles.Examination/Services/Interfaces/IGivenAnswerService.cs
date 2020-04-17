using Company.Pebbles.Examination.Models;
using System;

namespace Company.Pebbles.Examination.Services.Interfaces
{
    public interface IGivenAnswerService
    {
        TimeSpan ElapsedTime { get; }
        ExaminationAnswerModel GivenAnswer { get; }
        void SetCurrentQuestionIntoService(ExaminationQuestionModel question);
        bool IsNextAnswerNeeded(ExaminationAnswerModel selectedAnswer, TimeSpan elapsedTime);
        bool IsAnswerCorrect();
    }
}
