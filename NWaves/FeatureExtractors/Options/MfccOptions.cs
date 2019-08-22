using System.Collections.Generic;
using System.Runtime.Serialization;
using NWaves.Windows;

namespace NWaves.FeatureExtractors.Options
{
    [DataContract]
    public class MfccOptions : FilterbankOptions
    {
        [DataMember]
        public int LifterSize { get; set; }
        [DataMember]
        public string DctType { get; set; } = "2N";
        [DataMember]
        public bool IncludeEnergy { get; set; }
        [DataMember]
        public float LogEnergyFloor { get; set; } = float.Epsilon;

        public MfccOptions()
        {
            FilterBankSize = 24;
            NonLinearity = NonLinearityType.Log10;
            Window = WindowTypes.Hamming;
        }

        public override List<string> Errors
        {
            get
            {
                var errors = base.Errors;

                if (FilterBank == null && FilterBankSize < FeatureCount ||
                    FilterBank != null && FilterBank.Length < FeatureCount)
                {
                    errors.Add("Number of coefficients must not exceed number of filters");
                }

                var dctErrorText = "Supported DCT formats: 1, 2, 3, 4, 1N, 2N, 3N, 4N";

                if (string.IsNullOrEmpty(DctType) || DctType.Length > 2)
                {
                    errors.Add(dctErrorText);
                }
                else if (!"1234".Contains(DctType.Substring(0, 1)))
                {
                    errors.Add(dctErrorText);
                }
                else if (DctType.Length == 2 && char.ToUpper(DctType[1]) != 'N')
                {
                    errors.Add(dctErrorText);
                }

                return errors;
            }
        }
    }
}
