using LiveAudioPipelines.Core;
using LiveAudioPipelines.Forms.Controls;
using LiveAudioPipelines.Forms.Statics;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace LiveAudioPipelines.Forms.Modules
{
    public partial class AudioView : Form
    {
        public readonly AudioObj Audio;
        private AudioObj? sourceAudio;
        private AudioBag? sourceBag;
        public readonly AudioBag? OriginBag;

        private readonly System.Windows.Forms.Timer waveformTimer = new();
        private CancellationTokenSource? renderCts;
        private CancellationTokenSource? timeSeekCts;
        private Bitmap? waveformBitmap;
        private int renderVersion;
        private bool suppressOffsetScroll;
        private bool loopEnabled;
        private long viewOffsetSamples;
        private long? selectionStartSample;
        private long? selectionEndSample;
        private SelectionDragMode selectionDragMode;
        private long? lastManualSeekSample;
        private long lastPlaybackSampleIndex = -1;
        private const float PlaybackCaretAnchor = 0.5f;
        private const float SmoothScrollFactor = 0.25f;
        private const double ZoomStepFactor = 4d / 3d;
        private const int HandleSnapDistancePixels = 8;
        private const int EndScrollPaddingPixels = 96;
        private static float[]? selectionClipboard;

        private enum SelectionDragMode
        {
            None,
            Create,
            MoveStart,
            MoveEnd
        }


        public int SamplesPerPixel { get; private set; } = 256;
        public float Volume => 1 - (this.vScrollBar_volume.Value / (float)Math.Max(this.vScrollBar_volume.Maximum, 1));


        public AudioView(AudioObj audio, AudioBag? originBag = null)
        {
            this.InitializeComponent();
            WindowMain.Views.Add(this);
            this.sourceAudio = originBag != null ? audio : null;
            this.sourceBag = originBag;
            this.Audio = audio.Clone();
            this.OriginBag = originBag;
            this.Text = this.Audio.DisplayName + $" [{this.SamplesPerPixel} zoom]";

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            this.pictureBox_view.MouseWheel += this.pictureBox_view_MouseWheel;
            this.Resize += AudioViewEvents.AudioView_Resize;
            this.Resize += this.AudioView_ResizeRefresh;

            this.Load += this.AudioView_Load;
            this.FormClosing += this.AudioView_FormClosing;
            this.waveformTimer.Tick += this.waveformTimer_Tick;

        }

        private bool HasSelection => this.selectionStartSample.HasValue && this.selectionEndSample.HasValue && this.selectionStartSample.Value != this.selectionEndSample.Value;


        private async void AudioView_Load(object? sender, EventArgs e)
        {
            this.loopEnabled = false;
            this.button_loop.FlatStyle = FlatStyle.Standard;
            this.vScrollBar_volume.Value = 200;
            this.label_volume.Text = $"{(int)(this.Volume * 100)}%";

            await this.Audio.SetVolumeAsync(this.checkBox_mute.Checked ? 0f : this.Volume);
            await this.Audio.SetLoopEnabledAsync(this.loopEnabled);

            int fps = Math.Max(1, WindowMain.Settings.WaveformFps);
            this.waveformTimer.Interval = Math.Max(15, 1000 / fps);
            this.waveformTimer.Start();
            this.pictureBox_view.ContextMenuStrip = this.contextMenuSelection;

            this.SyncOffsetScrollBar();
            this.RequestWaveformRender();
            this.UpdatePlayButtonVisual();
        }

        private void RenameDisplayName()
        {
            string currentName = string.IsNullOrWhiteSpace(this.Audio.DisplayName) ? this.Audio.Name : this.Audio.DisplayName;
            string renamed = Interaction.InputBox("Enter new display name", "Rename Audio", currentName);
            if (string.IsNullOrWhiteSpace(renamed))
            {
                return;
            }

            this.Audio.DisplayName = renamed.Trim();
            this.Text = this.Audio.DisplayName + $" [{this.SamplesPerPixel} zoom]";
            this.RequestWaveformRender();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCLBUTTONDBLCLK = 0x00A3;
            const int HTCAPTION = 2;

            if (m.Msg == WM_NCLBUTTONDBLCLK && m.WParam.ToInt32() == HTCAPTION)
            {
                this.RenameDisplayName();
                return;
            }

            base.WndProc(ref m);
        }

        private void AudioView_FormClosing(object? sender, FormClosingEventArgs e)
        {
            this.waveformTimer.Stop();
            this.renderCts?.Cancel();
            this.timeSeekCts?.Cancel();
            this.timeSeekCts?.Dispose();
            this.timeSeekCts = null;

            var oldBitmap = this.waveformBitmap;
            this.waveformBitmap = null;
            oldBitmap?.Dispose();
            this.pictureBox_view.Image = null;

            _ = this.Audio.DisposeAsync().AsTask();
            WindowMain.Views.Remove(this);
        }

        private async void waveformTimer_Tick(object? sender, EventArgs e)
        {
            if (this.Audio.IsPlaying)
            {
                this.KeepPlaybackInView();
            }
            else
            {
                this.lastPlaybackSampleIndex = -1;
            }

            if (!this.textBox_time.Focused)
            {
                this.textBox_time.Text = this.Audio.CurrentPlaybackTime.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);
            }

            this.UpdatePlayButtonVisual();

            this.RequestWaveformRender();
        }

        private void AudioView_ResizeRefresh(object? sender, EventArgs e)
        {
            this.SyncOffsetScrollBar();
            this.RequestWaveformRender();
        }

        private void RequestWaveformRender()
        {
            if (this.pictureBox_view.Width <= 0 || this.pictureBox_view.Height <= 0)
            {
                return;
            }

            int version = Interlocked.Increment(ref this.renderVersion);

            this.renderCts?.Cancel();
            this.renderCts?.Dispose();
            this.renderCts = new CancellationTokenSource();

            _ = this.RenderWaveformAsync(version, this.renderCts.Token);
        }

        private async Task RenderWaveformAsync(int version, CancellationToken cancellationToken)
        {
            try
            {
                int width = this.pictureBox_view.Width;
                int height = this.pictureBox_view.Height;
                if (width <= 0 || height <= 0)
                {
                    return;
                }

                float caret = this.GetCaretPosition();
                Bitmap bitmap = await this.Audio.DrawWaveformAsync(
                    width,
                    height,
                    this.SamplesPerPixel,
                    this.viewOffsetSamples,
                    caret,
                    cancellationToken: cancellationToken);

                if (cancellationToken.IsCancellationRequested || version != this.renderVersion || this.IsDisposed)
                {
                    bitmap.Dispose();
                    return;
                }

                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() => this.SetRenderedBitmap(version, bitmap)));
                }
                else
                {
                    this.SetRenderedBitmap(version, bitmap);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void SetRenderedBitmap(int version, Bitmap bitmap)
        {
            if (this.IsDisposed || version != this.renderVersion)
            {
                bitmap.Dispose();
                return;
            }

            Bitmap? previous = this.waveformBitmap;
            this.waveformBitmap = bitmap;
            this.pictureBox_view.Image = bitmap;
            previous?.Dispose();
            this.pictureBox_view.Invalidate();
        }

        private float GetCaretPosition()
        {
            long visibleSamples = Math.Max(1, (long)this.pictureBox_view.Width * this.SamplesPerPixel);
            long current = this.Audio.CurrentSampleIndex;
            float position = (float)(current - this.viewOffsetSamples) / visibleSamples;
            return Math.Clamp(position, 0f, 1f);
        }

        private void KeepPlaybackInView()
        {
            long visibleSamples = Math.Max(1, (long)this.pictureBox_view.Width * this.SamplesPerPixel);
            long current = this.Audio.CurrentSampleIndex;
            long totalSamples = this.Audio.Samples.LongLength;
            long maxOffset = Math.Max(0, totalSamples - visibleSamples);
            long desiredOffset = Math.Clamp(current - (long)(visibleSamples * PlaybackCaretAnchor), 0, maxOffset);
            long delta = desiredOffset - this.viewOffsetSamples;
            float caretPosition = (float)(current - this.viewOffsetSamples) / Math.Max(1, visibleSamples);

            bool wrappedBack = this.lastPlaybackSampleIndex >= 0 && current + Math.Max(32, this.SamplesPerPixel * 4) < this.lastPlaybackSampleIndex;
            bool largeJump = this.lastPlaybackSampleIndex >= 0 && Math.Abs(current - this.lastPlaybackSampleIndex) > visibleSamples;
            bool caretOutOfView = caretPosition < 0f || caretPosition > 1f;
            bool caretFarFromCenter = caretPosition < 0.35f || caretPosition > 0.65f;

            if (wrappedBack || largeJump || caretOutOfView)
            {
                this.SetOffsetSamples(desiredOffset);
                this.lastPlaybackSampleIndex = current;
                return;
            }

            if (caretFarFromCenter)
            {
                this.SetOffsetSamples(desiredOffset);
                this.lastPlaybackSampleIndex = current;
                return;
            }

            if (delta == 0)
            {
                this.lastPlaybackSampleIndex = current;
                return;
            }

            long step = (long)(delta * Math.Max(SmoothScrollFactor, 0.4f));
            if (step == 0)
            {
                step = Math.Sign(delta);
            }

            this.SetOffsetSamples(this.viewOffsetSamples + step);
            this.lastPlaybackSampleIndex = current;
        }

        private void SyncOffsetScrollBar()
        {
            int viewportPixels = Math.Max(1, this.pictureBox_view.Width);
            this.hScrollBar_offset.LargeChange = viewportPixels;
            this.hScrollBar_offset.SmallChange = Math.Max(1, viewportPixels / 10);

            long totalSamples = this.Audio.Samples.LongLength;
            long visibleSamples = Math.Max(1, (long)viewportPixels * this.SamplesPerPixel);
            long maxOffsetSamples = Math.Max(0, totalSamples - visibleSamples + (long)EndScrollPaddingPixels * this.SamplesPerPixel);
            int maxOffsetPixels = (int)Math.Min(int.MaxValue, maxOffsetSamples / this.SamplesPerPixel);

            this.hScrollBar_offset.Minimum = 0;
            this.hScrollBar_offset.Maximum = maxOffsetPixels + this.hScrollBar_offset.LargeChange - 1;

            this.SetOffsetSamples(this.viewOffsetSamples);
        }

        private void SetOffsetSamples(long offsetSamples)
        {
            int maxScrollable = Math.Max(0, this.hScrollBar_offset.Maximum - this.hScrollBar_offset.LargeChange + 1);
            long maxSamples = (long)maxScrollable * this.SamplesPerPixel;
            long clamped = Math.Clamp(offsetSamples, 0, maxSamples);

            if (this.SamplesPerPixel > 1)
            {
                clamped = (clamped / this.SamplesPerPixel) * this.SamplesPerPixel;
            }

            this.viewOffsetSamples = clamped;

            int scrollValue = (int)Math.Clamp(this.viewOffsetSamples / this.SamplesPerPixel, 0, maxScrollable);

            this.suppressOffsetScroll = true;
            this.hScrollBar_offset.Value = scrollValue;
            this.suppressOffsetScroll = false;
        }


        private void pictureBox_view_MouseWheel(object? sender, MouseEventArgs e)
        {
            // Scroll back and forth in the audio timeline, if CTRL down zoom in and out (change SamplesPerPixel)
            if (Control.ModifierKeys == Keys.Control)
            {
                int oldSpp = this.SamplesPerPixel;
                long viewportSamplesBefore = Math.Max(1, (long)this.pictureBox_view.Width * oldSpp);
                long centerSampleBefore = this.viewOffsetSamples + (long)(viewportSamplesBefore * 0.5f);

                this.SamplesPerPixel = this.GetNextSamplesPerPixel(this.SamplesPerPixel, zoomIn: e.Delta > 0);

                this.Text = $"{this.Audio.DisplayName} [{this.SamplesPerPixel} zoom]";

                long viewportSamplesAfter = Math.Max(1, (long)this.pictureBox_view.Width * this.SamplesPerPixel);
                long desiredOffset;
                if (this.Audio.IsPlaying)
                {
                    long current = this.Audio.CurrentSampleIndex;
                    desiredOffset = current - (long)(viewportSamplesAfter * PlaybackCaretAnchor);
                }
                else
                {
                    desiredOffset = centerSampleBefore - (long)(viewportSamplesAfter * 0.5f);
                }

                this.SetOffsetSamples(desiredOffset);
                this.SyncOffsetScrollBar();
                this.RequestWaveformRender();
                return;
            }

            if (!this.Audio.IsPlaying)
            {
                long visibleSamples = Math.Max(1, (long)this.pictureBox_view.Width * this.SamplesPerPixel);
                long wheelStep = Math.Max(1, visibleSamples / 6);
                long deltaSamples = e.Delta > 0 ? -wheelStep : wheelStep;

                this.SetOffsetSamples(this.viewOffsetSamples + deltaSamples);
                this.RequestWaveformRender();
            }
        }

        private int GetNextSamplesPerPixel(int current, bool zoomIn)
        {
            current = Math.Clamp(current, 1, AudioViewEvents.MaxSamplesPerPixel);

            if (zoomIn)
            {
                double target = current / ZoomStepFactor;
                int next = (int)Math.Floor(target);
                if (next >= current)
                {
                    next = current - 1;
                }

                return Math.Clamp(next, 1, AudioViewEvents.MaxSamplesPerPixel);
            }

            double zoomOutTarget = current * ZoomStepFactor;
            int zoomOut = (int)Math.Ceiling(zoomOutTarget);
            if (zoomOut <= current)
            {
                zoomOut = current + 1;
            }

            return Math.Clamp(zoomOut, 1, AudioViewEvents.MaxSamplesPerPixel);
        }


        private async void pictureBox_view_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                return;
            }

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            long sample = this.SampleAtX(e.X);

            if (this.TryBeginHandleDrag(e.X))
            {
                return;
            }

            this.selectionDragMode = SelectionDragMode.Create;
            this.selectionStartSample = sample;
            this.selectionEndSample = sample;
            this.pictureBox_view.Invalidate();
        }

        private void pictureBox_view_MouseMove(object sender, MouseEventArgs e)
        {
            long sample = this.SampleAtX(e.X);

            if (this.selectionDragMode == SelectionDragMode.None)
            {
                this.pictureBox_view.Cursor = this.IsHandleHover(e.X) ? Cursors.SizeWE : Cursors.Default;
                return;
            }

            switch (this.selectionDragMode)
            {
                case SelectionDragMode.Create:
                    this.selectionEndSample = sample;
                    break;
                case SelectionDragMode.MoveStart:
                    this.selectionStartSample = sample;
                    break;
                case SelectionDragMode.MoveEnd:
                    this.selectionEndSample = sample;
                    break;
            }

            this.pictureBox_view.Invalidate();
        }

        private async void pictureBox_view_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (this.selectionDragMode == SelectionDragMode.None)
            {
                return;
            }

            SelectionDragMode dragMode = this.selectionDragMode;
            this.selectionDragMode = SelectionDragMode.None;

            if (!this.HasSelection)
            {
                this.selectionStartSample = null;
                this.selectionEndSample = null;

                long target = this.viewOffsetSamples + (long)e.X * this.SamplesPerPixel;
                this.lastManualSeekSample = Math.Clamp(target, 0, this.Audio.Samples.LongLength);
                await this.Audio.SeekSamplesAsync(target);
                this.RequestWaveformRender();
                return;
            }

            this.NormalizeSelectionOrder();
            this.pictureBox_view.Invalidate();

            if (this.loopEnabled && dragMode != SelectionDragMode.None)
            {
                _ = this.SyncLoopRangeAsync();
            }
        }

        private void pictureBox_view_Paint(object sender, PaintEventArgs e)
        {
            if (!this.TryGetSelectionRange(out long start, out long end))
            {
                return;
            }

            int x1 = this.XAtSample(start);
            int x2 = this.XAtSample(end);
            int left = Math.Max(0, Math.Min(x1, x2));
            int right = Math.Min(this.pictureBox_view.Width, Math.Max(x1, x2));
            int width = Math.Max(0, right - left);
            if (width <= 0)
            {
                return;
            }

            using Brush fill = new SolidBrush(Color.FromArgb(55, Color.DeepSkyBlue));
            using Pen border = new Pen(Color.DeepSkyBlue, 1f);
            using Pen handlePen = new Pen(Color.OrangeRed, 2f);

            e.Graphics.FillRectangle(fill, left, 0, width, this.pictureBox_view.Height);
            e.Graphics.DrawRectangle(border, left, 0, width - 1, this.pictureBox_view.Height - 1);
            e.Graphics.DrawLine(handlePen, left, 0, left, this.pictureBox_view.Height);
            e.Graphics.DrawLine(handlePen, right, 0, right, this.pictureBox_view.Height);
        }

        private bool TryGetSelectionRange(out long start, out long end)
        {
            start = 0;
            end = 0;
            if (!this.HasSelection)
            {
                return false;
            }

            start = Math.Min(this.selectionStartSample!.Value, this.selectionEndSample!.Value);
            end = Math.Max(this.selectionStartSample.Value, this.selectionEndSample.Value);
            return end > start;
        }

        private void NormalizeSelectionOrder()
        {
            if (!this.TryGetSelectionRange(out long start, out long end))
            {
                return;
            }

            this.selectionStartSample = start;
            this.selectionEndSample = end;
        }

        private long SampleAtX(int x)
        {
            int clampedX = Math.Clamp(x, 0, Math.Max(0, this.pictureBox_view.Width - 1));
            long sample = this.viewOffsetSamples + (long)clampedX * this.SamplesPerPixel;
            return Math.Clamp(sample, 0, this.Audio.Samples.LongLength);
        }

        private int XAtSample(long sample)
        {
            long relative = sample - this.viewOffsetSamples;
            return (int)Math.Clamp(relative / Math.Max(1, this.SamplesPerPixel), int.MinValue, int.MaxValue);
        }

        private bool IsHandleHover(int x)
        {
            if (!this.TryGetSelectionRange(out long start, out long end))
            {
                return false;
            }

            int left = this.XAtSample(start);
            int right = this.XAtSample(end);
            return Math.Abs(x - left) <= HandleSnapDistancePixels || Math.Abs(x - right) <= HandleSnapDistancePixels;
        }

        private bool TryBeginHandleDrag(int x)
        {
            if (!this.TryGetSelectionRange(out long start, out long end))
            {
                return false;
            }

            int left = this.XAtSample(start);
            int right = this.XAtSample(end);

            if (Math.Abs(x - left) <= HandleSnapDistancePixels)
            {
                this.selectionDragMode = SelectionDragMode.MoveStart;
                return true;
            }

            if (Math.Abs(x - right) <= HandleSnapDistancePixels)
            {
                this.selectionDragMode = SelectionDragMode.MoveEnd;
                return true;
            }

            return false;
        }

        private async Task SyncLoopRangeAsync()
        {
            if (!this.loopEnabled)
            {
                await this.Audio.SetLoopRangeAsync(null, null);
                return;
            }

            if (this.TryGetSelectionRange(out long start, out long end))
            {
                await this.Audio.SetLoopRangeAsync(start, end);
            }
            else
            {
                await this.Audio.SetLoopRangeAsync(null, null);
            }
        }

        private void UpdatePlayButtonVisual()
        {
            this.button_play.Text = this.Audio.IsPlaying ? "■" : "▶";
        }

        private async Task ApplyEditAsync(Action<long, long> editAction, bool rebuildPipeline)
        {
            if (!this.TryGetSelectionRange(out long start, out long end))
            {
                return;
            }

            await this.Audio.StopAsync();
            editAction(start, end);

            if (rebuildPipeline)
            {
                await this.Audio.ReloadPlaybackPipelineAsync();
            }

            long clampedStart = Math.Clamp(start, 0, this.Audio.Samples.LongLength);
            await this.Audio.SeekSamplesAsync(clampedStart);

            if (rebuildPipeline)
            {
                this.selectionStartSample = null;
                this.selectionEndSample = null;
            }
            else
            {
                long clampedEnd = Math.Clamp(end, 0, this.Audio.Samples.LongLength);
                if (clampedEnd > clampedStart)
                {
                    this.selectionStartSample = clampedStart;
                    this.selectionEndSample = clampedEnd;
                }
                else
                {
                    this.selectionStartSample = null;
                    this.selectionEndSample = null;
                }
            }

            await this.SyncLoopRangeAsync();
            this.SyncOffsetScrollBar();
            this.RequestWaveformRender();
            this.pictureBox_view.Invalidate();
            this.UpdatePlayButtonVisual();
        }

        private float? PromptFadeMinimum(string title)
        {
            string input = Interaction.InputBox("Minimum-Wert für Fade (0.0 bis 1.0)", title, "0.0");
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (!float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float value)
                && !float.TryParse(input, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
            {
                return null;
            }

            return Math.Clamp(value, 0f, 1f);
        }

        private bool TryParseTimeInput(out TimeSpan target)
        {
            if (TimeSpan.TryParse(this.textBox_time.Text, CultureInfo.InvariantCulture, out target)
                || TimeSpan.TryParse(this.textBox_time.Text, CultureInfo.CurrentCulture, out target))
            {
                target = TimeSpan.FromTicks(Math.Clamp(target.Ticks, TimeSpan.Zero.Ticks, this.Audio.Duration.Ticks));
                return true;
            }

            if (double.TryParse(this.textBox_time.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds)
                || double.TryParse(this.textBox_time.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out seconds))
            {
                target = TimeSpan.FromSeconds(seconds);
                target = TimeSpan.FromTicks(Math.Clamp(target.Ticks, TimeSpan.Zero.Ticks, this.Audio.Duration.Ticks));
                return true;
            }

            target = this.Audio.CurrentPlaybackTime;
            return false;
        }

        private async Task SeekToTimeTextboxAsync()
        {
            await this.SeekToTimeTextboxAsync(CancellationToken.None);
        }

        private async Task SeekToTimeTextboxAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!this.TryParseTimeInput(out TimeSpan target))
            {
                this.textBox_time.Text = this.Audio.CurrentPlaybackTime.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);
                return;
            }

            this.lastManualSeekSample = this.Audio.GetSampleIndexAtTime(target);
            await this.Audio.SeekAsync(target, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            this.RequestWaveformRender();
        }

        private void CopyAudioObjData(AudioObj source, AudioObj target)
        {
            target.FilePath = source.FilePath;
            target.Name = source.Name;
            target.DisplayName = source.DisplayName;
            target.SampleRate = source.SampleRate;
            target.Channels = source.Channels;
            target.Bpm = source.Bpm;
            target.ScannedBpm = source.ScannedBpm;
            target.Samples = (float[])source.Samples.Clone();
            target.Length = source.Length;
        }

        private void CommitEditsToOrigin()
        {
            if (this.sourceAudio != null
                && this.sourceBag != null
                && !this.sourceBag.IsDisposed
                && this.sourceBag.Audios.Audios.Contains(this.sourceAudio))
            {
                this.CopyAudioObjData(this.Audio, this.sourceAudio);
                this.sourceBag.RefreshAudioList();
                return;
            }

            AudioObj committed = this.Audio.Clone();
            AudioBag newBag = new AudioBag([committed]);
            this.sourceBag = newBag;
            this.sourceAudio = committed;
            newBag.RefreshAudioList();
        }

        private async void textBox_time_TextChanged(object sender, EventArgs e)
        {
            if (!this.textBox_time.Focused)
            {
                return;
            }

            this.timeSeekCts?.Cancel();
            this.timeSeekCts?.Dispose();
            this.timeSeekCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(180, this.timeSeekCts.Token);
                await this.SeekToTimeTextboxAsync(this.timeSeekCts.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task PasteClipboardAsync()
        {
            if (selectionClipboard == null || selectionClipboard.Length == 0)
            {
                return;
            }

            await this.Audio.StopAsync();

            long insertAt;
            if (this.TryGetSelectionRange(out long start, out long end))
            {
                insertAt = start;
                this.Audio.RemoveSampleRange(start, end);
            }
            else
            {
                insertAt = Math.Clamp(this.Audio.CurrentSampleIndex, 0, this.Audio.Samples.LongLength);
            }

            float[] clipCopy = (float[])selectionClipboard.Clone();
            this.Audio.InsertSamples(insertAt, clipCopy);
            await this.Audio.ReloadPlaybackPipelineAsync();
            await this.Audio.SeekSamplesAsync(insertAt);

            this.selectionStartSample = insertAt;
            this.selectionEndSample = insertAt + clipCopy.Length;

            await this.SyncLoopRangeAsync();
            this.SyncOffsetScrollBar();
            this.RequestWaveformRender();
            this.pictureBox_view.Invalidate();
            this.UpdatePlayButtonVisual();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData & Keys.Control) == Keys.Control && (keyData & Keys.KeyCode) == Keys.Back)
            {
                _ = this.Audio.SeekSamplesAsync(0);
                this.lastManualSeekSample = 0;
                this.RequestWaveformRender();
                return true;
            }

            if (keyData == Keys.Back)
            {
                long target = 0;
                if (this.TryGetSelectionRange(out long start, out _))
                {
                    target = start;
                }
                else if (this.lastManualSeekSample.HasValue)
                {
                    target = this.lastManualSeekSample.Value;
                }

                _ = this.Audio.SeekSamplesAsync(Math.Clamp(target, 0, this.Audio.Samples.LongLength));
                this.RequestWaveformRender();
                return true;
            }

            if ((keyData & Keys.Control) == Keys.Control && (keyData & Keys.KeyCode) == Keys.Space)
            {
                _ = this.Audio.StopAsync();
                this.UpdatePlayButtonVisual();
                this.RequestWaveformRender();
                return true;
            }

            if (keyData == Keys.Space)
            {
                _ = this.Audio.TogglePauseAsync();
                this.UpdatePlayButtonVisual();
                this.RequestWaveformRender();
                return true;
            }

            if (keyData == Keys.L)
            {
                this.button_loop_Click(this, EventArgs.Empty);
                return true;
            }

            if ((keyData & Keys.Control) == Keys.Control && (keyData & Keys.KeyCode) == Keys.C)
            {
                this.toolStripMenuItem_copy_Click(this, EventArgs.Empty);
                return true;
            }

            if ((keyData & Keys.Control) == Keys.Control && (keyData & Keys.KeyCode) == Keys.X)
            {
                this.toolStripMenuItem_cut_Click(this, EventArgs.Empty);
                return true;
            }

            if (keyData == Keys.Delete)
            {
                this.toolStripMenuItem_remove_Click(this, EventArgs.Empty);
                return true;
            }

            if ((keyData & Keys.Control) == Keys.Control && (keyData & Keys.KeyCode) == Keys.V)
            {
                _ = this.PasteClipboardAsync();
                return true;
            }

            if ((keyData & Keys.Control) == Keys.Control && (keyData & Keys.KeyCode) == Keys.N)
            {
                AudioObj empty = this.Audio.Clone();
                empty.DisplayName = $"{this.Audio.DisplayName} [New]";
                empty.Samples = [];
                empty.Length = 0;

                AudioView view = new AudioView(empty);
                view.Show();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void contextMenuSelection_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool enabled = this.HasSelection;
            this.toolStripMenuItem_copy.Enabled = enabled;
            this.toolStripMenuItem_cut.Enabled = enabled;
            this.toolStripMenuItem_silence.Enabled = enabled;
            this.toolStripMenuItem_remove.Enabled = enabled;
            this.toolStripMenuItem_normalize.Enabled = enabled;
            this.toolStripMenuItem_fadeIn.Enabled = enabled;
            this.toolStripMenuItem_fadeOut.Enabled = enabled;
        }

        private void toolStripMenuItem_copy_Click(object sender, EventArgs e)
        {
            if (!this.TryGetSelectionRange(out long start, out long end))
            {
                return;
            }

            selectionClipboard = this.Audio.GetSampleRangeCopy(start, end);
            Clipboard.SetText($"Audio selection copied: {selectionClipboard.Length} samples");
        }

        private async void toolStripMenuItem_cut_Click(object sender, EventArgs e)
        {
            if (!this.TryGetSelectionRange(out long start, out long end))
            {
                return;
            }

            selectionClipboard = this.Audio.GetSampleRangeCopy(start, end);
            Clipboard.SetText($"Audio selection cut: {selectionClipboard.Length} samples");
            await this.ApplyEditAsync((s, t) => this.Audio.RemoveSampleRange(s, t), rebuildPipeline: true);
        }

        private async void toolStripMenuItem_silence_Click(object sender, EventArgs e)
        {
            await this.ApplyEditAsync((s, t) => this.Audio.SilenceSampleRange(s, t), rebuildPipeline: false);
        }

        private async void toolStripMenuItem_remove_Click(object sender, EventArgs e)
        {
            await this.ApplyEditAsync((s, t) => this.Audio.RemoveSampleRange(s, t), rebuildPipeline: true);
        }

        private async void toolStripMenuItem_normalize_Click(object sender, EventArgs e)
        {
            await this.ApplyEditAsync((s, t) => this.Audio.NormalizeSampleRange(s, t), rebuildPipeline: false);
        }

        private async void toolStripMenuItem_fadeIn_Click(object sender, EventArgs e)
        {
            float? minValue = this.PromptFadeMinimum("Fade In");
            if (!minValue.HasValue)
            {
                return;
            }

            await this.ApplyEditAsync((s, t) => this.Audio.FadeSampleRange(s, t, minValue.Value, fadeIn: true), rebuildPipeline: false);
        }

        private async void toolStripMenuItem_fadeOut_Click(object sender, EventArgs e)
        {
            float? minValue = this.PromptFadeMinimum("Fade Out");
            if (!minValue.HasValue)
            {
                return;
            }

            await this.ApplyEditAsync((s, t) => this.Audio.FadeSampleRange(s, t, minValue.Value, fadeIn: false), rebuildPipeline: false);
        }

        private void checkBox_mute_CheckedChanged(object sender, EventArgs e)
        {
            _ = this.Audio.SetVolumeAsync(this.checkBox_mute.Checked ? 0f : this.Volume);
        }

        private void button_apply_Click(object sender, EventArgs e)
        {
            this.CommitEditsToOrigin();
        }

        private async void button_play_Click(object sender, EventArgs e)
        {
            if (this.Audio.IsPlaying)
            {
                await this.Audio.StopAsync();
            }
            else
            {
                await this.StartPlaybackFromLastManualSeekAsync();
            }

            this.UpdatePlayButtonVisual();
            this.RequestWaveformRender();
        }

        private async void button_pause_Click(object sender, EventArgs e)
        {
            await this.Audio.TogglePauseAsync();
            this.RequestWaveformRender();
        }

        private async void button_loop_Click(object sender, EventArgs e)
        {
            this.loopEnabled = !this.loopEnabled;
            this.button_loop.FlatStyle = this.loopEnabled ? FlatStyle.Popup : FlatStyle.Standard;
            await this.Audio.SetLoopEnabledAsync(this.loopEnabled);
            await this.SyncLoopRangeAsync();
        }

        private void textBox_time_Leave(object sender, EventArgs e)
        {
            _ = this.SeekToTimeTextboxAsync();
        }

        private void textBox_time_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
            _ = this.SeekToTimeTextboxAsync();
        }

        private void vScrollBar_volume_Scroll(object sender, ScrollEventArgs e)
        {
            this.label_volume.Text = $"{(int)(this.Volume * 100)}%";
            if (!this.checkBox_mute.Checked)
            {
                _ = this.Audio.SetVolumeAsync(this.Volume);
            }
        }

        private void hScrollBar_offset_Scroll(object sender, ScrollEventArgs e)
        {
            if (this.suppressOffsetScroll)
            {
                return;
            }

            this.viewOffsetSamples = (long)this.hScrollBar_offset.Value * this.SamplesPerPixel;
            this.RequestWaveformRender();
        }

        private async Task StartPlaybackFromLastManualSeekAsync()
        {
            if (this.lastManualSeekSample.HasValue)
            {
                await this.Audio.SeekSamplesAsync(this.lastManualSeekSample.Value);
            }

            await this.Audio.PlayAsync();
        }

        private async Task TogglePlaybackFromHotkeyAsync()
        {
            if (!this.Audio.IsPlaying && this.lastManualSeekSample.HasValue)
            {
                await this.Audio.SeekSamplesAsync(this.lastManualSeekSample.Value);
            }

            await this.Audio.TogglePauseAsync();
        }
    }
}
