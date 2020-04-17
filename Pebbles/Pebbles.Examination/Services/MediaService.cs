using Company.Pebbles.Configuration.Configuration;
using Company.Pebbles.Examination.Services.Interfaces;
using log4net;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;

namespace Company.Pebbles.Examination.Services
{
    internal class MediaService : IMediaService
    {
        private Action<float> setVolumeDelegate;
        private float volumeLevel;
        private readonly ILog logger;
        private readonly ISettings settings;
        private readonly IRemoteGeneralSettings generalSettings;
        protected WaveStream reader;
        private WaveOutEvent speakers;

        public TimeSpan CurrentAudioDuration { get => reader.TotalTime; }

        public MediaService(
            ILog logger,
            ISettings settings,
            IRemoteGeneralSettings generalSettings)
        {
            this.logger = logger;
            this.settings = settings;
            this.generalSettings = generalSettings;
        }

        public void StartAudioPlaying(string fileName, string filePath, float volume)
        {
            if (GetPlaybackState() == PlaybackState.Paused)
                Stop();

            PlayAudioFile(CreateFilePath(fileName, filePath), volume);
        }

        public void StopAudioPlaying()
        {
            if (GetPlaybackState() != PlaybackState.Playing)
                return;

            Stop();
        }

        public PlaybackState GetPlaybackState()
        {
            if (speakers == null)
                return PlaybackState.Stopped;

            return speakers.PlaybackState;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Stop()
        {
            speakers?.Stop();
            reader?.Dispose();
            speakers?.Dispose();
            logger.Info("Audio file is stopped");
        }

        public void ChangeVolume(float volume)
        {
            volumeLevel = volume;
            setVolumeDelegate?.Invoke(volume);
        }

        private void PlayAudioFile(string filePath, float volumeValue)
        {
            if (WaveOut.DeviceCount == 0)
                return;

            volumeLevel = volumeValue == default(float) ? volumeLevel : volumeValue;
            reader = new WaveFileReader(filePath);

            if (reader.WaveFormat.Encoding != WaveFormatEncoding.Pcm && reader.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            {
                reader = WaveFormatConversionStream.CreatePcmStream(reader);
                reader = new BlockAlignReductionStream(reader);
            }

            var sampleChannel = new SampleChannel(reader, true) { Volume = volumeLevel };
            setVolumeDelegate = delegate (float newVolume) { sampleChannel.Volume = newVolume; };

            speakers = new WaveOutEvent
            {
                Volume = generalSettings.Audio.Volume,
                DeviceNumber = settings.User.SpeakerDeviceNumber
            };
            speakers.Init(sampleChannel);

            speakers.PlaybackStopped += (sender, eventArgs) =>
            {
                if (ReferenceEquals(speakers, sender))
                    reader.Close();
            };

            speakers.Play();
        }

        private string CreateFilePath(string fileName, string filePath)
        {
            return Path.Combine(filePath, $"{fileName}.wav");
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;

            try
            {
                reader?.Dispose();
                speakers?.Dispose();
                logger.Info("Closing media service.");
                disposedValue = true;
            }
            catch (Exception ex)
            {
                logger.Error($"Error during closing audiochannel.", ex);
            }
        }
        #endregion
    }
}
