using Company.Pebbles.Examination.Models;

namespace Company.Pebbles.Examination.Services.AnswersAlgorithms.Interfaces
{
    public interface IAnswerStrategy
    {
        bool IsNextNeeded(ExaminationAnswerModel examinationAnswerModel, ExaminationAnswerModel selectedAnswer, int order);
        bool IsAnswerCorrect(ExaminationAnswerModel examinationAnswerModel, ExaminationAnswerModel[] givenAnswers);
    }
}
