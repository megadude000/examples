namespace Company.Pebbles.Examination.Models
{
    public class ExaminationSummaryModel
    {
        public int TotalTestCasesCount { get; set; }
        public int AnsweredTestCasesCount { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public int TimeUps { get; set; }
        public double CorrectnessPercentage { get; set; }
        public double AverageReactionTime { get; set; }
    }
}
