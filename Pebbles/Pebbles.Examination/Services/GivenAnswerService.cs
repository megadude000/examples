using Company.Pebbles.Examination.Models;
using Company.Pebbles.Examination.Services.AnswersAlgorithms;
using Company.Pebbles.Examination.Services.AnswersAlgorithms.Interfaces;
using Company.Pebbles.Examination.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Company.Pebbles.Examination.Services
{
    internal class GivenAnswerService : IGivenAnswerService
    {
        private ExaminationQuestionModel currentQuestion;
        private IAnswerStrategy answerStrategy { get; set; }
        private ExaminationAnswerModel firstGivenAnswer { get; set; }
        private List<ExaminationAnswerModel> totalGivenAnswers { get; set; } = new List<ExaminationAnswerModel>();


        public TimeSpan ElapsedTime { get; private set; } = new TimeSpan();
        public ExaminationAnswerModel GivenAnswer { get => GenerateAnswer(); }

        public void SetCurrentQuestionIntoService(ExaminationQuestionModel question)
        {
            currentQuestion = question;
            totalGivenAnswers.Clear();
        }

        public bool IsNextAnswerNeeded(ExaminationAnswerModel selectedAnswer, TimeSpan elapsedTime)
        {
            if (totalGivenAnswers.Count == 0)
            {
                ElapsedTime = elapsedTime;
                firstGivenAnswer = currentQuestion.ExpectedAnswers.SingleOrDefault(item => item.Id == selectedAnswer.Id);
                answerStrategy = (firstGivenAnswer != null) ? (firstGivenAnswer.NextAnswers != null) ? new MultiAnswerStrategy() : new SingleAnswerStrategy() as IAnswerStrategy : null;
            }
            var result = (firstGivenAnswer != null) ? answerStrategy.IsNextNeeded(firstGivenAnswer, selectedAnswer, totalGivenAnswers.Count) : false;

            totalGivenAnswers.Add((ExaminationAnswerModel)selectedAnswer.Clone());
            return result;
        }

        public bool IsAnswerCorrect()
        {
            return (firstGivenAnswer != null) ? answerStrategy.IsAnswerCorrect(firstGivenAnswer, totalGivenAnswers.ToArray()) : false;
        }

        private ExaminationAnswerModel GenerateAnswer()
        {
            var resultAnswer = new ExaminationAnswerModel();

            if (!totalGivenAnswers.Any())
                return resultAnswer;

            resultAnswer = totalGivenAnswers[0];
            if (totalGivenAnswers.Count() > 1)
            {
                resultAnswer.NextAnswers = totalGivenAnswers.Skip(1).ToArray();
            }

            return resultAnswer;
        }
    }
}
