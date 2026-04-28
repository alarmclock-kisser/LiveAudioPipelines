using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using LiveAudioPipelines.Shared;
using NAudio;

namespace LiveAudioPipelines.Core
{
    public class AudioCollection
    {
        public readonly BindingList<AudioObj> Audios = [];

        public readonly AudioExporter Exporter = new();




        public AudioCollection()
        {

        }



        public async Task<AudioObj?> ImportAudioAsync(string filePath)
        {
            try
            {
                AudioObj audio = await AudioObj.ImportAsync(filePath);
                if (audio != null)
                {
                    this.AddAudio(audio);
                    return audio;
                }
            }
            catch (Exception ex)
            {
                await StaticLogger.LogAsync($"Error importing audio file: {ex.Message}");
            }

            return null;
        }



        public void AddAudio(AudioObj audio)
        {
            if (audio != null)
            {
                this.Audios.Add(audio);
            }
        }

        public void AddAudios(IEnumerable<AudioObj?> audios)
        {
            foreach (var audio in audios)
            {
                if (audio != null)
                {
                    this.Audios.Add(audio);
                }
            }
        }

        public void RemoveAudio(Guid audioId, bool dispose = true)
        {
            var audio = this.Audios.FirstOrDefault(a => a.Id == audioId);
            if (audio != null)
            {
                this.Audios.Remove(audio);
                if (dispose)
                {
                    audio.DisposeAsync().AsTask().ConfigureAwait(false);
                }
            }
        }

        public void RemoveAudios(IEnumerable<Guid> audioIds, bool dispose = true)
        {
            foreach (var audioId in audioIds)
            {
                var audio = this.Audios.FirstOrDefault(a => a.Id == audioId);
                if (audio != null)
                {
                    this.Audios.Remove(audio);
                    if (dispose)
                    {
                        audio.DisposeAsync().AsTask().ConfigureAwait(false);
                    }
                }
            }
        }

        public void UpdateAudio(Guid audioId, AudioObj updatedAudio)
        {
            var audio = this.Audios.FirstOrDefault(a => a.Id == audioId);
            if (audio == null)
            {
                return;
            }

            audio.FilePath = updatedAudio.FilePath;
            audio.Name = updatedAudio.Name;
            audio.DisplayName = updatedAudio.DisplayName;
            audio.Samples = updatedAudio.Samples;
            audio.SampleRate = updatedAudio.SampleRate;
            audio.Channels = updatedAudio.Channels;
            audio.Length = updatedAudio.Length;
        }

        public async Task ClearAsync()
        {
            foreach (var audio in this.Audios)
            {
                await audio.DisposeAsync().ConfigureAwait(false);
            }

            this.Audios.Clear();
        }



    }
}
