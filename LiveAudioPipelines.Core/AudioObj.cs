using System;
using System.Collections.Generic;
using System.Text;
using NAudio;
using NAudio.Wave;
using TagLib;

namespace LiveAudioPipelines.Core
{
    public partial class AudioObj : IAsyncDisposable
    {
        public static readonly string[] SupportedExtensions = { ".wav", ".mp3", ".flac"};

        public readonly Guid Id = Guid.NewGuid();
        public string FilePath { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;


        public float[] Samples { get; set; } = [];
        public int SampleRate { get; set; } = 0;
        public int Channels { get; set; } = 0;
        public int BitDepth => 32;
        public long Length {  get; set; } = 0;
        public TimeSpan Duration => TimeSpan.FromSeconds((double)this.Length / (this.SampleRate * this.Channels * 4));


        public float Bpm { get; set; } = 0;
        public float? ScannedBpm { get; set; } = null;



        internal AudioObj(string filePath)
        {
            if (!System.IO.File.Exists(filePath) || !SupportedExtensions.Contains(Path.GetExtension(filePath).ToLower()))
            {
                return;
            }

            this.FilePath = Path.GetFullPath(filePath);
            this.Name = Path.GetFileNameWithoutExtension(filePath);
            this.DisplayName = this.Name;
        }

        internal static async Task<AudioObj> ImportAsync(string filePath)
        {
            var audioObj = new AudioObj(filePath);

            await Task.Run(() =>
            {
                try
                {
                    using var reader = new AudioFileReader(filePath);
                    audioObj.SampleRate = reader.WaveFormat.SampleRate;
                    audioObj.Channels = reader.WaveFormat.Channels;
                    audioObj.Length = reader.Length;
                    var sampleCount = (int)(reader.Length / (reader.WaveFormat.BitsPerSample / 8));
                    var samples = new float[sampleCount];
                    int readSamples = reader.Read(samples, 0, sampleCount);
                    audioObj.Samples = samples.Take(readSamples).ToArray();
                    var tagFile = TagLib.File.Create(filePath);
                    if (tagFile.Tag.BeatsPerMinute > 0)
                    {
                        audioObj.Bpm = tagFile.Tag.BeatsPerMinute;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing audio file: {ex.Message}");
                }
            });



            return audioObj;
        }

        public AudioObj Clone()
        {
            return new AudioObj(this.FilePath)
            {
                Name = this.Name,
                DisplayName = this.DisplayName,
                SampleRate = this.SampleRate,
                Channels = this.Channels,
                Length = this.Length,
                Samples = (float[])this.Samples.Clone(),
                Bpm = this.Bpm,
                ScannedBpm = this.ScannedBpm
            };
        }

        public (long Start, long End) GetClampedRange(long startSample, long endSample)
        {
            long max = this.Samples.LongLength;
            long start = Math.Clamp(Math.Min(startSample, endSample), 0, max);
            long end = Math.Clamp(Math.Max(startSample, endSample), 0, max);
            return (start, end);
        }

        public float[] GetSampleRangeCopy(long startSample, long endSample)
        {
            var range = this.GetClampedRange(startSample, endSample);
            int length = (int)Math.Clamp(range.End - range.Start, 0, int.MaxValue);
            if (length <= 0)
            {
                return [];
            }

            float[] copy = new float[length];
            Array.Copy(this.Samples, (int)range.Start, copy, 0, length);
            return copy;
        }

        public void SilenceSampleRange(long startSample, long endSample)
        {
            var range = this.GetClampedRange(startSample, endSample);
            for (long i = range.Start; i < range.End; i++)
            {
                this.Samples[i] = 0f;
            }
        }

        public void NormalizeSampleRange(long startSample, long endSample)
        {
            var range = this.GetClampedRange(startSample, endSample);
            if (range.End <= range.Start)
            {
                return;
            }

            float peak = 0f;
            for (long i = range.Start; i < range.End; i++)
            {
                float value = Math.Abs(this.Samples[i]);
                if (value > peak)
                {
                    peak = value;
                }
            }

            if (peak <= 0f)
            {
                return;
            }

            float gain = 1f / peak;
            for (long i = range.Start; i < range.End; i++)
            {
                this.Samples[i] = Math.Clamp(this.Samples[i] * gain, -1f, 1f);
            }
        }

        public void FadeSampleRange(long startSample, long endSample, float minValue, bool fadeIn)
        {
            minValue = Math.Clamp(minValue, 0f, 1f);
            var range = this.GetClampedRange(startSample, endSample);
            long length = range.End - range.Start;
            if (length <= 0)
            {
                return;
            }

            for (long i = 0; i < length; i++)
            {
                float t = length <= 1 ? 1f : i / (float)(length - 1);
                float gain = fadeIn
                    ? (minValue + (1f - minValue) * t)
                    : (1f - (1f - minValue) * t);

                int index = (int)(range.Start + i);
                this.Samples[index] = Math.Clamp(this.Samples[index] * gain, -1f, 1f);
            }
        }

        public void RemoveSampleRange(long startSample, long endSample)
        {
            var range = this.GetClampedRange(startSample, endSample);
            int removeLength = (int)Math.Clamp(range.End - range.Start, 0, int.MaxValue);
            if (removeLength <= 0)
            {
                return;
            }

            int start = (int)range.Start;
            float[] newSamples = new float[this.Samples.Length - removeLength];
            if (start > 0)
            {
                Array.Copy(this.Samples, 0, newSamples, 0, start);
            }

            int tailLength = this.Samples.Length - (start + removeLength);
            if (tailLength > 0)
            {
                Array.Copy(this.Samples, start + removeLength, newSamples, start, tailLength);
            }

            this.Samples = newSamples;
            this.Length = this.Samples.LongLength * sizeof(float);
        }

        public void InsertSamples(long insertAtSample, float[] samplesToInsert)
        {
            if (samplesToInsert == null || samplesToInsert.Length == 0)
            {
                return;
            }

            int insertAt = (int)Math.Clamp(insertAtSample, 0, this.Samples.Length);
            float[] newSamples = new float[this.Samples.Length + samplesToInsert.Length];

            if (insertAt > 0)
            {
                Array.Copy(this.Samples, 0, newSamples, 0, insertAt);
            }

            Array.Copy(samplesToInsert, 0, newSamples, insertAt, samplesToInsert.Length);

            int tailLength = this.Samples.Length - insertAt;
            if (tailLength > 0)
            {
                Array.Copy(this.Samples, insertAt, newSamples, insertAt + samplesToInsert.Length, tailLength);
            }

            this.Samples = newSamples;
            this.Length = this.Samples.LongLength * sizeof(float);
        }


        public async ValueTask DisposeAsync()
        {
            await this.DisposePlaybackAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }




    }
}
