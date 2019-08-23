using NWaves.Windows;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    [DataContract]
    public class FilterbankOptions : FeatureExtractorOptions
    {
        [DataMember]
        public float[][] FilterBank { get; set; }
        [DataMember]
        public int FilterBankSize { get; set; } = 12;
        [DataMember]
        public double LowFrequency { get; set; }
        [DataMember]
        public double HighFrequency { get; set; }
        [DataMember]
        public int FftSize { get; set; }
        [DataMember]
        public NonLinearityType NonLinearity { get; set; } = NonLinearityType.None;
        [DataMember]
        public SpectrumType SpectrumType { get; set; } = SpectrumType.Power;
        [DataMember]
        public float LogFloor { get; set; } = float.Epsilon;

        public FilterbankOptions() => Window = WindowTypes.Hamming;

        public override List<string> Errors
        {
            get
            {
                var errors = base.Errors;

                if (FilterBank == null && FilterBankSize <= 0)
                {
                    errors.Add("Positive number of filters must be specified");
                }

                return errors;
            }
        }
    }
}
