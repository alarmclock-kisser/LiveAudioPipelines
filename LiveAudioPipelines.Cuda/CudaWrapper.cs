using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using alarmclockkisser.AsynCUDA;
using LiveAudioPipelines.Shared;

namespace LiveAudioPipelines.Cuda
{
    public class CudaWrapper
    {
        private readonly CudaService Service = new();
        public BindingList<string> Devices => this.Service.DeviceEntries;
        public bool Initialized => this.Service.Initialized;

        public CudaWrapper()
        {

        }

        public void Initialize(int index)
        {
            this.Service.Initialize(index);
            StaticLogger.Log($"Initialized CUDA device: {this.Service.DeviceEntries[index]}");
        }

        public void Dispose()
        {
            this.Service.Dispose();
            StaticLogger.Log("Disposed CUDA service.");
        }



        public async Task<float[]> TimeStretchAsync(List<float[]> chunks, int chunkSize = 8192, float overlap = 0.5f)
        {
            if (!this.Initialized)
            {
                StaticLogger.Log("CUDA service not initialized. Call Initialize() before using.");
                return [];
            }

            if (chunks == null || chunks.Count == 0 || chunkSize <= 0 || overlap < 0 || overlap >= 1)
            {
                return [];
            }

            // FFT forward, keep pointer

            // Kernel on Complexes pointer

            // FFT inverse, pull and overlap-add + normalize
            return [];
        }

    }
}
