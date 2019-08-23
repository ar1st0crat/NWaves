using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    [DataContract]
    public class LpccOptions : LpcOptions
    {
        [DataMember]
        public int LifterSize { get; set; } = 22;

        public override List<string> Errors
        {
            get
            {
                var errors = base.Errors;
                if (FeatureCount <= 0) errors.Add("Positive number of LPCC coefficients must be specified");
                return errors;
            }
        }
    }
}
