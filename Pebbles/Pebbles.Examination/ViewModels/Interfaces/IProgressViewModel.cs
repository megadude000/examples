using Company.Pebbles.Examination.Enumerations;

namespace Company.Pebbles.Examination.ViewModels.Interfaces
{
    public interface IProgressViewModel
    {
        string CurrentProggressLabel { get; }

        void SetExamPassLabel();
        void SetCurrentProgress();
        void SetPressNextToStartLabel();
        void SetTotalTestCasesCount(int testCasesCount);
        void SetGivenAnswerStatusLabel(AnswerStatus answerStatus, bool isNextTestCaseExist);
    }
}
