using Company.Pebbles.Examination.Models;
using Company.Pebbles.Examination.Services.AnswersAlgorithms.Interfaces;
using System.Linq;

namespace Company.Pebbles.Examination.Services.AnswersAlgorithms
{
    internal class MultiAnswerStrategy : IAnswerStrategy
    {
        private const int parrent = 0;
        public bool IsAnswerCorrect(ExaminationAnswerModel examinationAnswerModel, ExaminationAnswerModel[] givenAnswers)
        {
            givenAnswers = givenAnswers.Skip(1).ToArray();

            for (int i = 0; i < examinationAnswerModel.NextAnswers.Length; i++)
            {
                if (examinationAnswerModel.NextAnswers[i].Id != givenAnswers[i].Id)
                    return false;
            }

            return true;
        }

        public bool IsNextNeeded(ExaminationAnswerModel examinationAnswerModel, ExaminationAnswerModel selectedAnswer, int order)
        {
            if (examinationAnswerModel.Id == selectedAnswer.Id && order == parrent)
                return true;

            if (examinationAnswerModel.NextAnswers[order - 1].Id == selectedAnswer.Id)
                return (order == examinationAnswerModel.NextAnswers.Length) ? false : true;

            return false;
        }
    }
}
