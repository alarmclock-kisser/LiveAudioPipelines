using LiveAudioPipelines.Forms.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace LiveAudioPipelines.Forms.Statics
{
    internal static class AudioBagEvents
    {
        public const int MaxHeight = 640;
        public const int MaxWidth = 800;
        public const int MinHeight = 80;
        public const int MinWidth = 220;
        public const int DefaultHeightOffset = 60;

        private static readonly Dictionary<AudioBag, ListChangedEventHandler> listChangedHandlers = new();
        private static readonly Dictionary<AudioBag, EventHandler> resizeHandlers = new();

        internal static void Bind(AudioBag bag)
        {
            if (!listChangedHandlers.ContainsKey(bag))
            {
                ListChangedEventHandler listChangedHandler = (_, _) => UpdateHeightToEntries(bag);
                listChangedHandlers[bag] = listChangedHandler;
                bag.Audios.Audios.ListChanged += listChangedHandler;
            }

            if (!resizeHandlers.ContainsKey(bag))
            {
                EventHandler resizeHandler = (_, _) => Invoke_AudioBag_Resized(bag);
                resizeHandlers[bag] = resizeHandler;
                bag.Resize += resizeHandler;
            }

            UpdateHeightToEntries(bag);
        }

        internal static void Unbind(AudioBag bag)
        {
            if (listChangedHandlers.TryGetValue(bag, out ListChangedEventHandler? listChangedHandler))
            {
                bag.Audios.Audios.ListChanged -= listChangedHandler;
                listChangedHandlers.Remove(bag);
            }

            if (resizeHandlers.TryGetValue(bag, out EventHandler? resizeHandler))
            {
                bag.Resize -= resizeHandler;
                resizeHandlers.Remove(bag);
            }
        }

        internal static void UpdateHeightToEntries(AudioBag bag)
        {
            int itemsHeight = GetItemsHeight(bag);
            int desiredHeight = GetFixedHeight(bag) + itemsHeight + bag.HeightOffset;
            desiredHeight = Math.Clamp(desiredHeight, MinHeight, MaxHeight);

            if (bag.Height != desiredHeight)
            {
                bag.Height = desiredHeight;
            }
        }

        internal static void Invoke_AudioBag_Resized(AudioBag bag)
        {
            if (bag.Locked)
            {
                return;
            }

            if (bag.Height > MaxHeight)
            {
                bag.Height = MaxHeight;
            }
            else if (bag.Height < MinHeight)
            {
                bag.Height = MinHeight;
            }

            if (bag.Width > MaxWidth)
            {
                bag.Width = MaxWidth;
            }
            else if (bag.Width < MinWidth)
            {
                bag.Width = MinWidth;
            }

            bag.HeightOffset = Math.Max(0, bag.ClientSize.Height - bag.ControlsHeight - GetItemsHeight(bag));

            bag.Refresh();
            bag.Invalidate();
        }

        private static int GetFixedHeight(AudioBag bag)
        {
            return (bag.Height - bag.ClientSize.Height) + bag.ControlsHeight;
        }

        private static int GetItemsHeight(AudioBag bag)
        {
            int itemHeight = Math.Max(bag.AudioItemHeight, 1);
            return Math.Max(itemHeight, bag.Audios.Audios.Count * itemHeight);
        }
    }
}
