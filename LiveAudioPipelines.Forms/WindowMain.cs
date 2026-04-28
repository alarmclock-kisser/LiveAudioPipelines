using LiveAudioPipelines.Core;
using LiveAudioPipelines.Forms.Modules;
using LiveAudioPipelines.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LiveAudioPipelines.Forms
{
    public partial class WindowMain : Form
    {
        public static AppSettings Settings { get; private set; } = new AppSettings();

        public static AudioCollection Audios { get; private set; } = new AudioCollection();

        public static List<AudioBag> Bags { get; private set; } = [];
        public static List<AudioView> Views { get; private set; } = [];

        private bool _logAddedSubscribed;



        public WindowMain(AppSettings appSettings)
        {
            this.InitializeComponent();
            Settings = appSettings;

            this.Load += this.WindowMain_Load;
            this.FormClosed += this.WindowMain_FormClosed;
            this.listBox_log.DataSource = StaticLogger.LogEntriesBindingList;
        }


        private void WindowMain_Load(object? sender, EventArgs e)
        {
            StaticLogger.SetUiContext(SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext());

            if (!_logAddedSubscribed)
            {
                StaticLogger.LogAdded += this.StaticLogger_LogAdded;
                _logAddedSubscribed = true;
            }

            this.toolStripComboBox_exportFormat.SelectedItem = Settings.ExportFormat;

            StaticLogger.Log("Application started.");
        }

        private void WindowMain_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (_logAddedSubscribed)
            {
                StaticLogger.LogAdded -= this.StaticLogger_LogAdded;
                _logAddedSubscribed = false;
            }
        }

        private void StaticLogger_LogAdded(string entry)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(this.ScrollLogToLatest));
                return;
            }

            this.ScrollLogToLatest();
        }

        private void ScrollLogToLatest()
        {
            if (this.listBox_log.Items.Count > 0)
            {
                this.listBox_log.TopIndex = this.listBox_log.Items.Count - 1;
            }
        }



        private void button_newBag_Click(object sender, EventArgs e)
        {
            AudioBag newBag = new AudioBag();
        }

        private async void button_import_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Audio Files|*.mp3;*.wav;*.flac";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var importTasks = openFileDialog.FileNames.Select(filePath => Audios.ImportAudioAsync(filePath)).ToArray();
                var importedAudios = await Task.WhenAll(importTasks);

                await StaticLogger.LogAsync($"Imported {importedAudios.Length} audio files.");

                AudioBag newBag = new(importedAudios);
                Audios.RemoveAudios(importedAudios.Select(a => a?.Id ?? Guid.Empty), false);
            }
        }

        private void toolStripComboBox_exportFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.toolStripComboBox_exportFormat.SelectedItem is string selectedFormat)
            {
                Settings.ExportFormat = selectedFormat;
                StaticLogger.Log($"Export format changed to: {selectedFormat}");

                int[] bitrates = AudioExporter.AvailableExportFormats.GetValueOrDefault(selectedFormat, new int[0]).OrderByDescending(b => b).ToArray();
                this.toolStripComboBox_bitrate.Items.Clear();
                this.toolStripComboBox_bitrate.Items.AddRange(bitrates.Cast<object>().ToArray());
                this.toolStripComboBox_bitrate.SelectedItem = bitrates.Contains(Settings.ExportBitrate) ? (object)Settings.ExportBitrate : (bitrates.Length > 0 ? (object)bitrates[0] : null);
            }
        }

        private void toolStripComboBox_bitrate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.toolStripComboBox_bitrate.SelectedItem is int selectedBitrate)
            {
                Settings.ExportBitrate = selectedBitrate;
                StaticLogger.Log($"Export bitrate changed to: {selectedBitrate}");
            }
        }

        private void toolStripTextBox_exportPath_Leave(object sender, EventArgs e)
        {
            string newPath = this.toolStripTextBox_exportPath.Text.Trim();

            if (!string.IsNullOrEmpty(newPath))
            {
                Settings.ExportDirectory = Path.GetFullPath(newPath);
                StaticLogger.Log($"Export path changed to: {newPath}");
            }
        }
    }
}
