using Company.Pebbles.Examination.Enumerations;
using Company.Pebbles.Examination.Properties;
using Company.Pebbles.Examination.ViewModels.Interfaces;
using PropertyChanged;

namespace Company.Pebbles.Examination.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ProgressViewModel : IProgressViewModel
    {
        private int currentIteration;
        private int totalTestCasesCount;

        public string CurrentProggressLabel { get; private set; }

        public void SetTotalTestCasesCount(int testCasesCount) => totalTestCasesCount = testCasesCount;

        public void SetExamPassLabel() => CurrentProggressLabel = Resources.ExamSuccessfullyPassedLabel;

        public void SetPressNextToStartLabel() => CurrentProggressLabel = Resources.PressNextToStartLabel;

        public void SetCurrentProgress() => CurrentProggressLabel = $"{Resources.Task}:{++currentIteration}/{totalTestCasesCount}";

        public void SetGivenAnswerStatusLabel(AnswerStatus answerStatus, bool isNextTestCaseExist)
        {
            switch (answerStatus)
            {
                case AnswerStatus.Correct:
                    CurrentProggressLabel = $"{Resources.Correct}! ";
                    break;
                case AnswerStatus.Incorrect:
                    CurrentProggressLabel = $"{Resources.Incorrect}! ";
                    break;
                case AnswerStatus.TimeUp:
                    CurrentProggressLabel = $"{Resources.TimeUp}! ";
                    break;
                default:
                    break;
            }

            CurrentProggressLabel += isNextTestCaseExist ? Resources.PressNextToContinue : Resources.PressStopToFinishLabel;
        }
    }
}
