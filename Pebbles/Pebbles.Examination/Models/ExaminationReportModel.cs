using Company.Pebbles.Examination.Enumerations;
using System;

namespace Company.Pebbles.Examination.Models
{
    public class ExaminationReportModel
    {
        public long QuestionId { get; set; }
        public string TestCaseName { get; set; }
        public TimeSpan SpentTime { get; set; }
        public ExaminationAnswerModel Answer { get; set; }
        public AnswerStatus Status { get; set; }
    }
}
