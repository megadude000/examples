using System;

namespace Company.Pebbles.Examination.Models
{
    public class ExaminationAnswerModel: AnswerModel, ICloneable
    {
        public AnswerModel[] NextAnswers { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
