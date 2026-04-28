using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace LiveAudioPipelines.Core
{
    public partial class AudioObj
    {
        public int ChunkSize { get; set; } = 0;
        public int OverlapSize { get; set; } = 0;

        public async Task<List<float[]>> GetChunksAsync(int chunkSize = 8192, float overlap = 0.5f, int? maxWorkers = null, bool keepData = true)
        {
            if (chunkSize <= 0 || overlap < 0 || overlap >= 1)
            {
                return [];
            }

            maxWorkers ??= Environment.ProcessorCount;
            maxWorkers = Math.Clamp(maxWorkers.Value, 1, Environment.ProcessorCount);

            // Async & parallel get chunks from Samples with specified chunkSize and overlap
            // Align overlap to channel count to keep L-R frame boundaries intact
            int ch = Math.Max(1, this.Channels);
            if (ch > 1 && this.OverlapSize % ch != 0)
            {
                this.OverlapSize = (this.OverlapSize / ch) * ch;
            }

            int step = chunkSize - this.OverlapSize;
            if (step <= 0)
            {
                return [];
            }

            int numChunks = Math.Max(1, ((this.Samples.Length - chunkSize) / step) + 1);
            float[][] chunks = new float[numChunks][];

            await Task.Run(() =>
            {
                Parallel.For(0, numChunks, new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxWorkers.Value,
                }, i =>
                {
                    int sourceOffset = i * step;
                    float[] chunk = new float[chunkSize];
                    Buffer.BlockCopy(
                        src: this.Samples,
                        srcOffset: sourceOffset * sizeof(float),
                        dst: chunk,
                        dstOffset: 0,
                        count: chunkSize * sizeof(float));
                    chunks[i] = chunk;
                });
            }).ConfigureAwait(false);

            if (!keepData)
            {
                this.Samples = [];
            }

            return chunks.ToList();
        }





    }
}
