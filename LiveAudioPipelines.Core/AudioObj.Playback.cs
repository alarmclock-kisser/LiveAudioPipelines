using System;
using System.Collections.Concurrent;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace LiveAudioPipelines.Core
{
    public partial class AudioObj
    {
        private readonly object playbackSync = new();
        private readonly object workerSync = new();

        private readonly record struct PlaybackCommand(Action Action, TaskCompletionSource Completion);

        private BlockingCollection<PlaybackCommand>? playbackCommands;
        private Thread? playbackWorker;

        private WaveOutEvent? waveOut;
        private PlaybackSampleProvider? playbackSource;
        private VolumeSampleProvider? volumeProvider;
        private bool stopRequested;
        private float playbackVolume = 1f;
        private bool loopEnabled;
        private long? loopStartSample;
        private long? loopEndSample;
        private PlaybackState playbackState = PlaybackState.Stopped;

        public event EventHandler? PlaybackStateChanged;

        public PlaybackState CurrentPlaybackState
        {
            get
            {
                lock (this.playbackSync)
                {
                    return this.playbackState;
                }
            }
        }

        public bool IsPlaying => this.CurrentPlaybackState == PlaybackState.Playing;

        public long CurrentSampleIndex => this.playbackSource?.GetPosition() ?? 0;

        public TimeSpan CurrentPlaybackTime => this.SampleRate <= 0 || this.Channels <= 0
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds((double)this.CurrentSampleIndex / (this.SampleRate * this.Channels));

        public float PlaybackVolume
        {
            get
            {
                lock (this.playbackSync)
                {
                    return this.playbackVolume;
                }
            }
        }

        public bool LoopEnabled
        {
            get
            {
                lock (this.playbackSync)
                {
                    return this.loopEnabled;
                }
            }
        }

        public Task PlayAsync(CancellationToken cancellationToken = default)
        {
            return this.EnqueuePlaybackCommandAsync(() =>
            {
                this.EnsurePlaybackPipeline();

                if (this.waveOut == null)
                {
                    return;
                }

                this.waveOut.Play();
                this.SetPlaybackState(PlaybackState.Playing);
            }, cancellationToken);
        }

        public Task TogglePauseAsync(CancellationToken cancellationToken = default)
        {
            return this.EnqueuePlaybackCommandAsync(() =>
            {
                this.EnsurePlaybackPipeline();

                if (this.waveOut == null)
                {
                    return;
                }

                if (this.playbackState == PlaybackState.Playing)
                {
                    this.waveOut.Pause();
                    this.SetPlaybackState(PlaybackState.Paused);
                }
                else
                {
                    this.waveOut.Play();
                    this.SetPlaybackState(PlaybackState.Playing);
                }
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            return this.EnqueuePlaybackCommandAsync(() =>
            {
                if (this.waveOut != null)
                {
                    this.stopRequested = true;
                    this.waveOut.Stop();
                }

                this.playbackSource?.Seek(0);
                this.SetPlaybackState(PlaybackState.Stopped);
            }, cancellationToken);
        }

        public Task SeekAsync(TimeSpan target, CancellationToken cancellationToken = default)
        {
            return this.SeekSamplesAsync(this.GetSampleIndexAtTime(target), cancellationToken);
        }

        public Task SeekSamplesAsync(long sampleIndex, CancellationToken cancellationToken = default)
        {
            return this.EnqueuePlaybackCommandAsync(() =>
            {
                this.EnsurePlaybackPipeline();
                this.playbackSource?.Seek(sampleIndex);
            }, cancellationToken);
        }

        public Task SetLoopEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
        {
            return this.EnqueuePlaybackCommandAsync(() =>
            {
                this.loopEnabled = enabled;
                if (this.playbackSource != null)
                {
                    this.playbackSource.LoopEnabled = enabled;
                }
            }, cancellationToken);
        }

        public Task SetLoopRangeAsync(long? startSample, long? endSample, CancellationToken cancellationToken = default)
        {
            return this.EnqueuePlaybackCommandAsync(() =>
            {
                if (startSample.HasValue && endSample.HasValue)
                {
                    var range = this.GetClampedRange(startSample.Value, endSample.Value);
                    if (range.End > range.Start)
                    {
                        this.loopStartSample = range.Start;
                        this.loopEndSample = range.End;
                    }
                    else
                    {
                        this.loopStartSample = null;
                        this.loopEndSample = null;
                    }
                }
                else
                {
                    this.loopStartSample = null;
                    this.loopEndSample = null;
                }

                this.playbackSource?.SetLoopRange(this.loopStartSample, this.loopEndSample);
            }, cancellationToken);
        }

        public Task SetVolumeAsync(float volume, CancellationToken cancellationToken = default)
        {
            volume = Math.Clamp(volume, 0f, 1f);
            return this.EnqueuePlaybackCommandAsync(() =>
            {
                this.playbackVolume = volume;
                if (this.volumeProvider != null)
                {
                    this.volumeProvider.Volume = volume;
                }
            }, cancellationToken);
        }

        public Task ReloadPlaybackPipelineAsync(CancellationToken cancellationToken = default)
        {
            return this.EnqueuePlaybackCommandAsync(() =>
            {
                if (this.waveOut != null)
                {
                    this.stopRequested = true;
                    this.waveOut.Stop();
                    this.waveOut.PlaybackStopped -= this.WaveOut_PlaybackStopped;
                    this.waveOut.Dispose();
                    this.waveOut = null;
                }

                this.playbackSource = null;
                this.volumeProvider = null;
                this.playbackState = PlaybackState.Stopped;
            }, cancellationToken);
        }

        private void EnsurePlaybackPipeline()
        {
            lock (this.playbackSync)
            {
                if (this.waveOut != null)
                {
                    return;
                }

                if (this.Samples.Length == 0 || this.SampleRate <= 0 || this.Channels <= 0)
                {
                    return;
                }

                this.playbackSource = new PlaybackSampleProvider(this.Samples, this.SampleRate, this.Channels)
                {
                    LoopEnabled = this.loopEnabled
                };
                this.playbackSource.SetLoopRange(this.loopStartSample, this.loopEndSample);

                this.volumeProvider = new VolumeSampleProvider(this.playbackSource)
                {
                    Volume = this.playbackVolume
                };

                var waveProvider = new SampleToWaveProvider16(this.volumeProvider);

                this.waveOut = new WaveOutEvent
                {
                    DesiredLatency = 40,
                    NumberOfBuffers = 3
                };

                this.waveOut.PlaybackStopped += this.WaveOut_PlaybackStopped;
                this.waveOut.Init(waveProvider);
            }
        }

        private void WaveOut_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            lock (this.playbackSync)
            {
                if (this.stopRequested)
                {
                    this.stopRequested = false;
                    return;
                }

                if (this.playbackSource != null && !this.playbackSource.LoopEnabled)
                {
                    this.playbackSource.Seek(0);
                }

                this.SetPlaybackState(PlaybackState.Stopped);
            }
        }

        private Task EnqueuePlaybackCommandAsync(Action action, CancellationToken cancellationToken)
        {
            this.EnsurePlaybackWorker();

            if (this.playbackCommands == null)
            {
                return Task.CompletedTask;
            }

            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken));

            try
            {
                this.playbackCommands.Add(new PlaybackCommand(action, completion), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                completion.TrySetCanceled(cancellationToken);
            }
            catch (InvalidOperationException)
            {
                completion.TrySetException(new ObjectDisposedException(nameof(AudioObj)));
            }

            return completion.Task;
        }

        private void EnsurePlaybackWorker()
        {
            lock (this.workerSync)
            {
                if (this.playbackWorker != null)
                {
                    return;
                }

                this.playbackCommands = new BlockingCollection<PlaybackCommand>();
                this.playbackWorker = new Thread(this.PlaybackWorkerLoop)
                {
                    Name = $"AudioPlaybackWorker-{this.Id}",
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };

                this.playbackWorker.Start();
            }
        }

        private void PlaybackWorkerLoop()
        {
            if (this.playbackCommands == null)
            {
                return;
            }

            foreach (var command in this.playbackCommands.GetConsumingEnumerable())
            {
                if (command.Completion.Task.IsCanceled)
                {
                    continue;
                }

                try
                {
                    command.Action();
                    command.Completion.TrySetResult();
                }
                catch (Exception ex)
                {
                    command.Completion.TrySetException(ex);
                }
            }
        }

        private void SetPlaybackState(PlaybackState state)
        {
            bool changed;
            lock (this.playbackSync)
            {
                changed = this.playbackState != state;
                this.playbackState = state;
            }

            if (changed)
            {
                this.PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private async ValueTask DisposePlaybackAsync()
        {
            BlockingCollection<PlaybackCommand>? commands;
            Thread? worker;

            lock (this.workerSync)
            {
                commands = this.playbackCommands;
                worker = this.playbackWorker;
                this.playbackCommands = null;
                this.playbackWorker = null;
            }

            if (commands != null)
            {
                commands.CompleteAdding();
            }

            if (worker != null && worker.IsAlive)
            {
                await Task.Run(worker.Join).ConfigureAwait(false);
            }

            lock (this.playbackSync)
            {
                if (this.waveOut != null)
                {
                    this.waveOut.PlaybackStopped -= this.WaveOut_PlaybackStopped;
                    this.waveOut.Dispose();
                    this.waveOut = null;
                }

                this.playbackSource = null;
                this.volumeProvider = null;
                this.playbackState = PlaybackState.Stopped;
            }
        }

        private sealed class PlaybackSampleProvider : ISampleProvider
        {
            private readonly float[] samples;
            private readonly object sync = new();
            private long position;
            private long? loopStartSample;
            private long? loopEndSample;

            public PlaybackSampleProvider(float[] samples, int sampleRate, int channels)
            {
                this.samples = samples;
                this.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
            }

            public WaveFormat WaveFormat { get; }

            public bool LoopEnabled { get; set; }

            public void SetLoopRange(long? startSample, long? endSample)
            {
                lock (this.sync)
                {
                    if (startSample.HasValue && endSample.HasValue)
                    {
                        long start = Math.Clamp(Math.Min(startSample.Value, endSample.Value), 0, this.samples.Length);
                        long end = Math.Clamp(Math.Max(startSample.Value, endSample.Value), 0, this.samples.Length);
                        if (end > start)
                        {
                            this.loopStartSample = start;
                            this.loopEndSample = end;
                            return;
                        }
                    }

                    this.loopStartSample = null;
                    this.loopEndSample = null;
                }
            }

            private (long Start, long End) GetPlaybackBounds()
            {
                if (this.LoopEnabled && this.loopStartSample.HasValue && this.loopEndSample.HasValue)
                {
                    return (this.loopStartSample.Value, this.loopEndSample.Value);
                }

                return (0, this.samples.Length);
            }

            public int Read(float[] buffer, int offset, int count)
            {
                lock (this.sync)
                {
                    int copied = 0;
                    var bounds = this.GetPlaybackBounds();

                    if (this.position < bounds.Start)
                    {
                        this.position = bounds.Start;
                    }

                    while (copied < count)
                    {
                        int available = (int)Math.Max(0, bounds.End - this.position);
                        if (available <= 0)
                        {
                            if (!this.LoopEnabled)
                            {
                                break;
                            }

                            this.position = bounds.Start;
                            available = (int)Math.Max(0, bounds.End - this.position);
                            if (available <= 0)
                            {
                                break;
                            }
                        }

                        int toCopy = Math.Min(count - copied, available);
                        Array.Copy(this.samples, (int)this.position, buffer, offset + copied, toCopy);
                        copied += toCopy;
                        this.position += toCopy;
                    }

                    return copied;
                }
            }

            public long GetPosition()
            {
                lock (this.sync)
                {
                    return this.position;
                }
            }

            public void Seek(long sampleIndex)
            {
                lock (this.sync)
                {
                    this.position = Math.Clamp(sampleIndex, 0, this.samples.Length);
                }
            }
        }
    }
}
