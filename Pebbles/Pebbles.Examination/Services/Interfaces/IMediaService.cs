using System;
using NAudio.Wave;

namespace Company.Pebbles.Examination.Services.Interfaces
{
    public interface IMediaService : IDisposable
    {
        TimeSpan CurrentAudioDuration { get; }

        void StopAudioPlaying();
        PlaybackState GetPlaybackState();
        void StartAudioPlaying(string fileName, string filePath, float volume);
        void ChangeVolume(float volume);
    }
}
