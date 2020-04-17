using Company.Pebbles.Examination.Models;
using Company.Pebbles.Examination.Services.AnswersAlgorithms.Interfaces;

namespace Company.Pebbles.Examination.Services.AnswersAlgorithms
{
    internal class SingleAnswerStrategy : IAnswerStrategy
    {
        public bool IsAnswerCorrect(ExaminationAnswerModel examinationAnswerModel, ExaminationAnswerModel[] givenAnswers)
        {
            return givenAnswers[0].Id == examinationAnswerModel.Id;
        }

        public bool IsNextNeeded(ExaminationAnswerModel examinationAnswerModel, ExaminationAnswerModel selectedAnswer, int order)
        {
            return false;
        }
    }
}
