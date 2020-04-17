using Company.Pebbles.Examination.Models;

namespace Company.Pebbles.Examination.ViewModels.Events
{
    public class ShowSummary
    {
        public ExaminationSummaryModel Summary { get; set; }

        public ShowSummary(ExaminationSummaryModel summaryModel)
        {
            Summary = summaryModel;
        }
    }
}
