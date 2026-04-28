using alarmclockkisser.DragNDrop.Forms;
using LiveAudioPipelines.Core;
using LiveAudioPipelines.Forms.Statics;
using LiveAudioPipelines.Shared;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static alarmclockkisser.DragNDrop.Forms.ListBoxExtensions;

namespace LiveAudioPipelines.Forms.Modules
{
    public partial class AudioBag : Form
    {
        private const int WM_NCLBUTTONDBLCLK = 0x00A3;
        private const int HTCAPTION = 2;


        public readonly AudioCollection Audios = new();

        private AudioInfoPreview? CurrentPreview;
        private int CurrentHoverIndex = -1;


        public bool ShowPreview => this.checkBox_preview.Checked;
        public bool AutoPlay => this.checkBox_autoPlay.Checked;
        public bool Locked => this.checkBox_lock.Checked;
        internal int HeightOffset = 80;
        internal int ControlsHeight => this.panel_controls.Height;
        internal int AudioItemHeight => this.listBox_audios.ItemHeight;



        public AudioBag(IEnumerable<AudioObj?>? audios = null)
        {
            this.InitializeComponent();
            WindowMain.Bags.Add(this);
            this.DoubleBuffered = true;

            if (audios != null)
            {
                this.Audios.AddAudios(audios);
            }

            Register_ListBox_DragNDrop(this.listBox_audios, true);
            this.listBox_audios.DataSource = this.Audios.Audios;
            this.listBox_audios.DrawMode = DrawMode.OwnerDrawFixed;
            this.listBox_audios.DrawItem += this.listBox_audios_DrawItem;
            this.listBox_audios.MouseMove += this.listBox_audios_MouseMove;
            this.listBox_audios.MouseLeave += this.listBox_audios_MouseLeave;
            this.Resize += this.AudioBag_Resize;
            AudioBagEvents.Bind(this);

            this.FormClosing += this.AudioBag_FormClosing;
            this.Show();
        }



        private async void AudioBag_FormClosing(object? sender, FormClosingEventArgs e)
        {
            this.CurrentPreview?.Dispose();
            this.CurrentPreview = null;
            AudioBagEvents.Unbind(this);
            await this.Audios.ClearAsync();
            WindowMain.Bags.Remove(this);
        }

        private void AudioBag_Resize(object? sender, EventArgs e)
        {
            this.UpdateHoveredPreviewPosition();
        }

        private void checkBox_lock_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Locked)
            {
                this.TopMost = true;
                this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            }
            else
            {
                this.TopMost = false;
                this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            }
        }

        private void listBox_audios_DrawItem(object? sender, DrawItemEventArgs e)
        {
            // Sicherheitsprüfung, falls die Liste leer ist oder neu gezeichnet wird
            if (e.Index < 0 || e.Index >= this.Audios.Audios.Count)
            {
                return;
            }

            AudioObj audio = this.Audios.Audios[e.Index];

            // 1. Hintergrund zeichnen (dies kümmert sich automatisch um die blaue Markierung, wenn das Item ausgewählt ist)
            e.DrawBackground();

            // 2. Texte vorbereiten (Ich gehe davon aus, dass Duration ein TimeSpan ist)
            string durationText = audio.Duration.ToString(@"mm\:ss");
            string nameText = audio.DisplayName;

            // 3. Richtige Textfarbe wählen (Weiß bei Selektion, sonst Schwarz/Standard)
            Brush textBrush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                ? SystemBrushes.HighlightText
                : SystemBrushes.WindowText;

            // 4. Layout berechnen
            // Wir messen, wie viel Breite der Duration-Text benötigt
            SizeF durationSize = e.Graphics.MeasureString(durationText, e.Font ?? SystemFonts.DefaultFont);

            // Wir definieren ein Rechteck ganz rechts für die Dauer (mit 2 Pixel Rand)
            RectangleF durationRect = new RectangleF(
                e.Bounds.Right - durationSize.Width - 2,
                e.Bounds.Y,
                durationSize.Width,
                e.Bounds.Height);

            // Wir definieren ein Rechteck für den Namen, das den gesamten *restlichen* Platz einnimmt
            RectangleF nameRect = new RectangleF(
                e.Bounds.Left + 2,
                e.Bounds.Y,
                e.Bounds.Width - durationSize.Width - 4, // Gesamtbreite minus Dauer minus Padding
                e.Bounds.Height);

            // 5. String-Formate für das Verhalten des Textes definieren
            using (StringFormat nameFormat = new StringFormat())
            using (StringFormat durationFormat = new StringFormat())
            {
                // Name-Format: Vertikal zentrieren, kein Umbruch, bei Platzmangel mit "..." abschneiden
                nameFormat.LineAlignment = StringAlignment.Center;
                nameFormat.FormatFlags = StringFormatFlags.NoWrap;
                nameFormat.Trimming = StringTrimming.EllipsisCharacter;

                // Dauer-Format: Vertikal zentrieren, rechtsbündig
                durationFormat.LineAlignment = StringAlignment.Center;
                durationFormat.Alignment = StringAlignment.Far;

                // 6. Texte in die definierten Rechtecke zeichnen
                e.Graphics.DrawString(nameText, e.Font ?? SystemFonts.DefaultFont, textBrush, nameRect, nameFormat);
                e.Graphics.DrawString(durationText, e.Font ?? SystemFonts.DefaultFont, textBrush, durationRect, durationFormat);
            }

            // 7. Fokus-Rechteck zeichnen (die feine gestrichelte Linie, wenn man mit der Tastatur durchnavigiert)
            e.DrawFocusRectangle();
        }

        private void listBox_audios_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!this.ShowPreview)
            {
                this.HidePreview();
                return;
            }

            int index = this.listBox_audios.IndexFromPoint(e.Location);
            if (index < 0 || index >= this.Audios.Audios.Count)
            {
                this.HidePreview();
                this.CurrentHoverIndex = -1;
                return;
            }

            if (index != this.CurrentHoverIndex || this.CurrentPreview == null || this.CurrentPreview.IsDisposed || !this.CurrentPreview.Visible)
            {
                this.CurrentHoverIndex = index;
                this.ShowOrUpdatePreview(index);
                return;
            }

            this.UpdateHoveredPreviewPosition();
        }

        private void listBox_audios_MouseLeave(object? sender, EventArgs e)
        {
            this.CurrentHoverIndex = -1;
            this.HidePreview();
        }

        private void ShowOrUpdatePreview(int index)
        {
            AudioObj? audio = this.Audios.Audios.ElementAtOrDefault(index);
            if (audio == null)
            {
                this.HidePreview();
                return;
            }

            Rectangle itemRect = this.listBox_audios.GetItemRectangle(index);
            if (itemRect.IsEmpty)
            {
                this.HidePreview();
                return;
            }

            if (this.CurrentPreview == null || this.CurrentPreview.IsDisposed)
            {
                this.CurrentPreview = new AudioInfoPreview(audio);
                this.CurrentPreview.Show(this);
            }
            else
            {
                this.CurrentPreview.UpdateAudio(audio);
                if (!this.CurrentPreview.Visible)
                {
                    this.CurrentPreview.Show(this);
                }
            }

            this.UpdatePreviewLocation(itemRect);
        }

        private void UpdateHoveredPreviewPosition()
        {
            if (this.CurrentHoverIndex < 0 || this.CurrentPreview == null || this.CurrentPreview.IsDisposed || !this.CurrentPreview.Visible)
            {
                return;
            }

            Rectangle itemRect = this.listBox_audios.GetItemRectangle(this.CurrentHoverIndex);
            if (itemRect.IsEmpty)
            {
                this.HidePreview();
                return;
            }

            this.UpdatePreviewLocation(itemRect);
        }

        private void UpdatePreviewLocation(Rectangle itemRect)
        {
            if (this.CurrentPreview == null || this.CurrentPreview.IsDisposed)
            {
                return;
            }

            Point screenLocation = this.listBox_audios.PointToScreen(new Point(this.listBox_audios.Width + 8, itemRect.Top));
            this.CurrentPreview.Location = screenLocation;
            this.CurrentPreview.BringToFront();
        }

        private void HidePreview()
        {
            if (this.CurrentPreview != null && !this.CurrentPreview.IsDisposed)
            {
                this.CurrentPreview.Hide();
            }
        }

        private void listBox_audios_DoubleClick(object sender, EventArgs e)
        {
            int index = this.listBox_audios.IndexFromPoint(this.listBox_audios.PointToClient(Cursor.Position));
            if (index >= 0 && index < this.Audios.Audios.Count)
            {
                AudioObj audio = this.Audios.Audios[index];
                AudioView view = new AudioView(audio, this);
                view.Show();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCLBUTTONDBLCLK && m.WParam.ToInt32() == HTCAPTION)
            {
                this.RenameBag();
                return;
            }

            base.WndProc(ref m);
        }

        private void RenameBag()
        {
            string currentName = string.IsNullOrWhiteSpace(this.Text) ? "AudioBag" : this.Text;
            string renamed = Interaction.InputBox("Enter new bag name", "Rename AudioBag", currentName);
            if (string.IsNullOrWhiteSpace(renamed))
            {
                return;
            }

            this.Text = renamed.Trim();
        }

        private async void button_export_Click(object sender, EventArgs e)
        {
            if (this.Audios.Audios.Count == 0)
            {
                MessageBox.Show("No audio tracks to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedAudios = this.listBox_audios.SelectedItems.Cast<AudioObj>().ToArray();
            if (selectedAudios.Length == 0)
            {
                MessageBox.Show("Please select at least one track to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool useCustomOutput = ModifierKeys.HasFlag(Keys.Control);
            string? outputDirectory = null;

            if (useCustomOutput)
            {
                using FolderBrowserDialog dialog = new()
                {
                    Description = "Select export folder",
                    SelectedPath = string.IsNullOrWhiteSpace(WindowMain.Settings.ExportDirectory)
                        ? WindowMain.Audios.Exporter.ExportDirectory
                        : WindowMain.Settings.ExportDirectory,
                    UseDescriptionForTitle = true
                };

                if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    return;
                }

                outputDirectory = dialog.SelectedPath;
            }

            string? exportFormat = WindowMain.Settings.ExportFormat;
            bool wantsMp3 = string.Equals(exportFormat?.Trim(), ".mp3", StringComparison.OrdinalIgnoreCase)
                || string.Equals(exportFormat?.Trim(), "mp3", StringComparison.OrdinalIgnoreCase);
            bool wantsWav = string.IsNullOrWhiteSpace(exportFormat)
                || string.Equals(exportFormat?.Trim(), ".wav", StringComparison.OrdinalIgnoreCase)
                || string.Equals(exportFormat?.Trim(), "wav", StringComparison.OrdinalIgnoreCase);

            async Task<string?> ExportOneAsync(AudioObj audio)
            {
                try
                {
                    if (wantsMp3)
                    {
                        string? mp3Result = await this.Audios.Exporter.ExportMp3Async(audio, outDir: outputDirectory);
                        if (!string.IsNullOrWhiteSpace(mp3Result))
                        {
                            return mp3Result;
                        }
                    }
                    else if (wantsWav)
                    {
                        string? wavResult = await this.Audios.Exporter.ExportWavAsync(audio, bitDepth: 24, outDir: outputDirectory);
                        if (!string.IsNullOrWhiteSpace(wavResult))
                        {
                            return wavResult;
                        }
                    }

                    string? fallback = await this.Audios.Exporter.ExportWavAsync(audio, bitDepth: 16, outDir: outputDirectory);
                    return fallback;
                }
                catch (Exception ex)
                {
                    await StaticLogger.LogAsync($"Export failed for '{audio.DisplayName}': {ex.Message}. Falling back to WAV 16-bit.");
                    try
                    {
                        return await this.Audios.Exporter.ExportWavAsync(audio, bitDepth: 16, outDir: outputDirectory);
                    }
                    catch (Exception fallbackEx)
                    {
                        await StaticLogger.LogAsync($"Fallback WAV export failed for '{audio.DisplayName}': {fallbackEx.Message}");
                        return null;
                    }
                }
            }

            string?[] results = await Task.WhenAll(selectedAudios.Select(ExportOneAsync));
            int exportedCount = results.Count(path => !string.IsNullOrWhiteSpace(path));

            if (exportedCount == 0)
            {
                MessageBox.Show("Export failed for all selected tracks.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await StaticLogger.LogAsync($"Exported {exportedCount} track(s) from bag '{this.Text}'.");
        }

        internal void RefreshAudioList()
        {
            this.listBox_audios.Refresh();
            this.Invalidate();
        }

    }
}
