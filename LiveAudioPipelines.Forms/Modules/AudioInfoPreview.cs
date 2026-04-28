using LiveAudioPipelines.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LiveAudioPipelines.Forms.Modules
{
    public partial class AudioInfoPreview : Form
    {
        public AudioObj Audio { get; private set; }
        private int PreviewVersion;

        public AudioInfoPreview(AudioObj audio)
        {
            InitializeComponent();
            this.Audio = audio;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;

            this.UpdateAudio(audio);
        }



        public void UpdateAudio(AudioObj audio)
        {
            this.Audio = audio;
            this.textBox_info.Text = $"{audio.DisplayName}\r\n{audio.Duration:mm\\:ss}\r\n{audio.Bpm:F3}";
            int version = ++this.PreviewVersion;
            this.LoadPreviewAsync(audio, version);
        }

        private async void LoadPreviewAsync(AudioObj audio, int version)
        {
            if (audio.Duration.TotalMilliseconds > WindowMain.Settings.MaxPreviewDurationMs)
            {
                return;
            }

            try
            {
                var image = await audio.GetPreviewAsync(this.pictureBox_preview.Width, this.pictureBox_preview.Height);
                if (this.IsDisposed || version != this.PreviewVersion)
                {
                    image?.Dispose();
                    return;
                }

                var oldImage = this.pictureBox_preview.Image;
                this.pictureBox_preview.Image = image;
                oldImage?.Dispose();
            }
            catch
            {
            }
        }
    }
}
