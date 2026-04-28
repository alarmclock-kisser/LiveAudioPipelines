using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LiveAudioPipelines.Core
{
    public partial class AudioObj
    {
        public readonly CustomTags Tags = new();


        public Dictionary<string, double> Metrics { get; } = [];

        public double this[string metric]
        {
            get
            {
                if (this.Metrics.TryGetValue(metric, out double value))
                {
                    return value;
                }

                var key = this.Metrics.Keys.FirstOrDefault(k => k.Equals(metric, StringComparison.OrdinalIgnoreCase));
                return key != null ? this.Metrics[key] : 0.0;
            }
            set
            {
                if (this.Metrics.ContainsKey(metric))
                {
                    this.Metrics[metric] = value;
                    return;
                }

                var key = this.Metrics.Keys.FirstOrDefault(k => k.Equals(metric, StringComparison.OrdinalIgnoreCase));
                if (key != null)
                {
                    this.Metrics[key] = value;
                    return;
                }

                string capitalizedMetric = metric.Length > 0
                    ? char.ToUpper(metric[0], CultureInfo.InvariantCulture) + metric[1..].ToLowerInvariant()
                    : metric;
                this.Metrics.Add(capitalizedMetric, value);
            }
        }



    }
}
