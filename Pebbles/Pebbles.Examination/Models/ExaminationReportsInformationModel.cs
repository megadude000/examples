using Company.Pebbles.Configuration.Models.Configuration;
using System;
using System.Collections.Generic;

namespace Company.Pebbles.Examination.Models
{
    public class ExaminationReportsInformationModel
    {
        public long ExaminationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public UserInformation UserInformation { get; set; }
        public int AllAnswers { get; set; }
        public int AllQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int FailedAnswers { get; set; }
        public int TimeUps { get; set; }
        public double AverageReactionTime { get; set; }
        public ExaminationReportModel[] ExaminationReports { get; set; }
        public long InstanceId { get; set; }
    }
}
