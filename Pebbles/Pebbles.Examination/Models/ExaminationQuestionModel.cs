namespace Company.Pebbles.Examination.Models
{
    public class ExaminationQuestionModel
    {
        public long QuestionId { get; set; }
        public double DefaultReactionTime { get; set; }
        public double MaxReactionTime { get; set; }
        public string AudioFileName { get; set; }
        public bool IsRandom { get; set; }
        public ExaminationAnswerModel[] ExpectedAnswers { get; set; }
    }
}
