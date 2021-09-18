using NWaves.Windows;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    [DataContract]
    public class PlpOptions : FilterbankOptions
    {
        /// <summary>
        /// Order of LPC (0, by default, i.e. it will be autocomputed as FeatureCount-1).
        /// </summary>
        [DataMember]
        public int LpcOrder { get; set; }

        /// <summary>
        /// Coefficient of RASTA-filter (0, by default, i.e. there will be no RASTA-filtering).
        /// </summary>
        [DataMember]
        public double Rasta { get; set; }

        /// <summary>
        /// Number of liftered coefficients (0, by default, i.e. there will be no liftering).
        /// </summary>
        [DataMember]
        public int LifterSize { get; set; }
        
        [DataMember]
        public double[] CenterFrequencies { get; set; }

        [DataMember]
        public bool IncludeEnergy { get; set; }

        [DataMember]
        public float LogEnergyFloor { get; set; } = float.Epsilon;

        public PlpOptions()
        {
            FilterBankSize = 24;
            Window = WindowType.Hamming;
        }

        public override List<string> Errors
        {
            get
            {
                var errors = base.Errors;
                if (FeatureCount <= 0) errors.Add("Positive number of PLP coefficients must be specified");
                return errors;
            }
        }
    }
}
