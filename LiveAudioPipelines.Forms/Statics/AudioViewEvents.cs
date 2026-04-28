using LiveAudioPipelines.Forms.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAudioPipelines.Forms.Statics
{
    internal static class AudioViewEvents
    {
        public const int MinHeight = 80;
        public const int MaxSamplesPerPixel = 65536;

        internal static void AudioView_Resize(object? sender, EventArgs e)
        {
            if (sender is AudioView audioView)
            {
                if (audioView.Height < MinHeight)
                {
                    audioView.Height = MinHeight;
                }

                audioView.Invalidate();
            }
        }
    }
}
