using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAudioPipelines.Shared
{
    public class AppSettings
    {
        public bool CreateLogFile { get; set; } = true;
        public string LogFilePath { get; set; } = string.Empty;
        public int MaxLogFiles { get; set; } = 1;


        public int MaxPreviewDurationMs { get; set; } = 10000;
        public int WaveformFps { get; set; } = 24;

        public string ExportDirectory { get; set; } = string.Empty;
        public string ExportFormat { get; set; } = "wav";
        public int ExportBitrate { get; set; } = 16;


    }
}
