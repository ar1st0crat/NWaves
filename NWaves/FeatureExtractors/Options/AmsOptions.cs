using System.Collections.Generic;

namespace NWaves.FeatureExtractors.Options
{
    public class AmsOptions : FeatureExtractorOptions
    {
        public int ModulationFftSize { get; set; } = 64;
        public int ModulationHopSize { get; set; } = 4;
        public int FftSize { get; set; }
        public IEnumerable<float[]> Featuregram { get; set; }
        public float[][] FilterBank { get; set; }
    }
}
