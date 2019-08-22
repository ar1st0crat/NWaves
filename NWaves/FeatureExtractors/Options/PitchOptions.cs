using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    [DataContract]
    public class PitchOptions : FeatureExtractorOptions
    {
        [DataMember]
        public double LowFrequency { get; set; } = 80;/*Hz*/
        [DataMember]
        public double HighFrequency { get; set; } = 400;/*Hz*/

        public override List<string> Errors
        {
            get
            {
                var errors = base.Errors;
                if (LowFrequency >= HighFrequency) errors.Add("Upper frequency must be greater than lower frequency");
                return errors;
            }
        }
    }
}
