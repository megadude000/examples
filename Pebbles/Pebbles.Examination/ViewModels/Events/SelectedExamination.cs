namespace Company.Pebbles.Examination.ViewModels.Events
{
    public class SelectedExamination
    {
        public long ExaminationId { get; private set; }

        public SelectedExamination(long examinationId)
        {
            ExaminationId = examinationId;
        }
    }
}
