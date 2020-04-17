using System;

namespace Company.Pebbles.Examination.Services.Interfaces
{
    public interface IExaminationTimer
    {
        bool TimerActive { get; }
        TimeSpan Stop();
        TimeSpan GetElapsedTime();
        event Action TimeUp;
        event Action<int> ProgressChanged;
        void Start(TimeSpan maximumReactionTime);
    }
}
