using Company.Pebbles.Examination.Enumerations;

namespace Company.Pebbles.Examination.Models
{
    public class AnswerModel
    {
        public long Id { get; set; }
        public string Value { get; set; }
        public AnswerType Type { get; set; }
    }
}
