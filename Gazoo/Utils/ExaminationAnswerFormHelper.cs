using Company.Gazoo.Database.Entities.Examination;
using Company.Gazoo.Models.Examinations;
using Company.Gazoo.Models.Examinations.RequestModels;
using System.Collections.Generic;
using System.Linq;

namespace Company.Gazoo.Utils
{
    public static class ExaminationAnswerHelper
    {
        public static AnswerModel FormSelectedAnswer(IEnumerable<QuestionResultAnswers> answers)
        {
            return new AnswerModel
            {
                Id = answers.First().SelectedAnswer.Id,
                IsDeleted = answers.First().SelectedAnswer.IsDeleted,
                Name = answers.First().SelectedAnswer.Name,
                Type = answers.First().SelectedAnswer.Type,
                NextAnswers = (answers.Count() > 1) ? FormNextSelectedAnswers(answers) : null
            };
        }

        public static AnswerModel[] FormAnswers(IEnumerable<QuestionAnswer> answers)
        {
            return answers
                .Where(answer => answer.IsHead)
                .Select(answer => new AnswerModel
                {
                    Id = answer.Answer.Id,
                    IsDeleted = answer.Answer.IsDeleted,
                    Name = answer.Answer.Name,
                    Type = answer.Answer.Type,
                    NextAnswers = (!answer.NextAnswerId.HasValue) ? null : FormNextAnswers(ref answers, answer.NextAnswerId)
                })
                .ToArray();
        }

        public static ExaminationAnswerModel[] FormExpectedAnswers(IEnumerable<QuestionAnswer> answers)
        {
            return answers
                .Where(answer => answer.IsHead)
                .Select(answer => new ExaminationAnswerModel
                {
                    Id = answer.Answer.Id,
                    Value = answer.Answer.Name,
                    Type = answer.Answer.Type,
                    NextAnswers = (!answer.NextAnswerId.HasValue) ? null : FormNextExpectedAnswers(ref answers, answer.NextAnswerId)
                })
                .ToArray();
        }

        private static BaseAnswerModel[] FormNextExpectedAnswers(ref IEnumerable<QuestionAnswer> answers, long? nextAnswerId)
        {
            List<BaseAnswerModel> result = new List<BaseAnswerModel>();

            while (nextAnswerId.HasValue)
            {
                var answer = answers.Where(entity => entity.AnswerId == nextAnswerId.Value).Single();
                result.Add(new BaseAnswerModel
                {
                    Id = answer.Answer.Id,
                    Type = answer.Answer.Type,
                    Value = answer.Answer.Name
                });
                nextAnswerId = answer.NextAnswerId;
            }

            return result.ToArray();
        }

        private static Answer[] FormNextAnswers(ref IEnumerable<QuestionAnswer> answers, long? nextAnswerId)
        {
            List<Answer> result = new List<Answer>();

            while (nextAnswerId.HasValue)
            {
                var answer = answers.Where(entity => entity.AnswerId == nextAnswerId.Value).Single();
                result.Add(answer.Answer);
                nextAnswerId = answer.NextAnswerId;
            }

            return result.ToArray();
        }

        private static Answer[] FormNextSelectedAnswers(IEnumerable<QuestionResultAnswers> answer)
        {
            return answer
                    .Skip(1)
                    .Select(item => item.SelectedAnswer)
                    .ToArray();
        }
    }
}
