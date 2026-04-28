using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Versioning;
using System.Threading;



namespace LiveAudioPipelines.Core
{
    public partial class AudioObj
    {
        public long Offset { get; set; } = 0;



        public long GetSampleIndexAtTime(TimeSpan time)
        {
            return (long)(time.TotalSeconds * this.SampleRate * this.Channels);
        }





        [SupportedOSPlatform("windows")]
        public async Task<Bitmap> GetPreviewAsync(int width, int height, Color? graphColor = null, Color? backgroundColor = null)
        {
            graphColor ??= Color.Black;
            backgroundColor ??= Color.White;

            // Async draw from float Samples to Bitmap
            return await Task.Run(() =>
            {
                Bitmap bitmap = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(backgroundColor.Value);
                    if (this.Samples != null && this.Samples.Length > 0)
                    {
                        int samplesPerPixel = Math.Max(1, this.Samples.Length / width);
                        for (int x = 0; x < width; x++)
                        {
                            int startSample = x * samplesPerPixel;
                            int endSample = Math.Min(startSample + samplesPerPixel, this.Samples.Length);
                            float maxSample = 0;
                            for (int i = startSample; i < endSample; i++)
                            {
                                maxSample = Math.Max(maxSample, Math.Abs(this.Samples[i]));
                            }
                            int y = (int)((1 - maxSample) * height / 2);
                            using Pen pen = new Pen(graphColor.Value);
                            g.DrawLine(pen, x, height / 2 - y, x, height / 2 + y);
                        }
                    }
                }
                return bitmap;
            });
        }

        [SupportedOSPlatform("windows")]
        public async Task<Bitmap> DrawWaveformAsync(int width, int height, int samplesPerPixel, long offset = 0, float caretPosition = 0.5f, Color? graphColor = null, Color? backgroundColor = null, Color? caretColor = null, CancellationToken cancellationToken = default)
        {
            graphColor ??= Color.Black;
            backgroundColor ??= Color.White;
            caretColor ??= Color.Red;
            const int MaxProbeCount = 96;
            return await Task.Run(() =>
            {
                Bitmap bitmap = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.Clear(backgroundColor.Value);
                    if (this.Samples != null && this.Samples.Length > 0)
                    {
                        samplesPerPixel = Math.Max(1, samplesPerPixel);
                        float halfHeight = height / 2f;
                        using Pen graphPen = new Pen(graphColor.Value);
                        using Pen averagePen = new Pen(Color.FromArgb(190, graphColor.Value), 1f);
                        int? previousY = null;
                        int? previousAverageY = null;

                        for (int x = 0; x < width; x++)
                        {
                            if ((x & 31) == 0)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return bitmap;
                                }
                            }

                            long startIndex = offset + (long)x * samplesPerPixel;
                            if (startIndex >= this.Samples.Length)
                                break;

                            int start = (int)Math.Max(0, startIndex);
                            int end = Math.Min(start + samplesPerPixel, this.Samples.Length);

                            int sampleCount = Math.Max(1, end - start);
                            int probeStep = sampleCount > MaxProbeCount ? Math.Max(1, sampleCount / MaxProbeCount) : 1;

                            float min = 1f;
                            float max = -1f;
                            float sum = 0f;
                            int probes = 0;
                            for (int i = start; i < end; i += probeStep)
                            {
                                float sample = this.Samples[i];
                                if (sample < min)
                                {
                                    min = sample;
                                }
                                if (sample > max)
                                {
                                    max = sample;
                                }

                                sum += sample;
                                probes++;
                            }

                            if (probeStep > 1 && end - 1 >= start)
                            {
                                float tailSample = this.Samples[end - 1];
                                if (tailSample < min)
                                {
                                    min = tailSample;
                                }
                                if (tailSample > max)
                                {
                                    max = tailSample;
                                }
                                sum += tailSample;
                                probes++;
                            }

                            if (probes <= 0)
                            {
                                continue;
                            }

                            int yTop = (int)(halfHeight - (max * halfHeight));
                            int yBottom = (int)(halfHeight - (min * halfHeight));

                            if (samplesPerPixel == 1)
                            {
                                int y = (int)(halfHeight - (this.Samples[start] * halfHeight));
                                y = Math.Clamp(y, 0, Math.Max(0, height - 1));
                                if (previousY.HasValue)
                                {
                                    g.DrawLine(graphPen, x - 1, previousY.Value, x, y);
                                }
                                else
                                {
                                    g.DrawLine(graphPen, x, y, x, y);
                                }

                                previousY = y;
                                previousAverageY = y;
                            }
                            else
                            {
                                g.DrawLine(graphPen, x, yTop, x, yBottom);

                                float avg = sum / probes;
                                int avgY = (int)(halfHeight - (avg * halfHeight));
                                avgY = Math.Clamp(avgY, 0, Math.Max(0, height - 1));
                                if (previousAverageY.HasValue)
                                {
                                    g.DrawLine(averagePen, x - 1, previousAverageY.Value, x, avgY);
                                }

                                previousAverageY = avgY;
                            }
                        }

                        // Draw caret
                        int caretX = (int)(Math.Clamp(caretPosition, 0f, 1f) * width);
                        using Pen caretPen = new Pen(caretColor.Value, 2);
                        g.DrawLine(caretPen, caretX, 0, caretX, height);
                    }
                }
                return bitmap;
            });
        }


    }
}
