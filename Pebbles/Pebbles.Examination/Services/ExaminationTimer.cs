using Company.Pebbles.Examination.Services.Interfaces;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Company.Pebbles.Examination.Services
{
    internal class ExaminationTimer : IExaminationTimer, IDisposable
    {
        private Timer reactionTimer;
        private TimeSpan reactionTime;
        private Stopwatch stopWatchTimer;

        public event Action TimeUp;
        public event Action<int> ProgressChanged;
        public bool TimerActive { get; private set; } = false;

        public ExaminationTimer()
        {
            reactionTimer = new Timer();
            stopWatchTimer = new Stopwatch();
        }

        public void Start(TimeSpan maximumReactionTime)
        {
            if (reactionTimer.Enabled)
                return;

            stopWatchTimer.Reset();
            reactionTimer = new Timer();
            reactionTime = maximumReactionTime;
            var interval = TimeSpan.FromMilliseconds((reactionTime.TotalMilliseconds / 300)).TotalMilliseconds;
            reactionTimer.Interval = (int)interval;
            reactionTimer.Tick += new EventHandler(ReactionTimerTickHandler);
            reactionTimer.Start();
            stopWatchTimer.Start();
            TimerActive = true;
        }

        public TimeSpan Stop()
        {
            stopWatchTimer.Stop();
            reactionTimer.Stop();
            TimerActive = false;
            return stopWatchTimer.Elapsed;
        }

        public TimeSpan GetElapsedTime()
        {
            return stopWatchTimer.Elapsed;
        }

        private void ReactionTimerTickHandler(object sender, EventArgs e)
        {
            if (reactionTime > stopWatchTimer.Elapsed)
            {
                var step = reactionTime.TotalSeconds / 100;
                var currentProgress = 100 - (int)(stopWatchTimer.Elapsed.TotalSeconds / step);
                ProgressChanged?.Invoke(currentProgress);
            }
            else
            {
                Stop();
                TimeUp?.Invoke();
            }
        }

        public void Dispose()
        {
            reactionTimer.Dispose();
        }
    }
}
