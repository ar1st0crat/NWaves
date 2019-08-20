using System.Collections.Generic;

namespace NWaves.FeatureExtractors.Options
{
    public class PitchOptions : FeatureExtractorOptions
    {
        public double LowFrequency { get; set; } = 80;/*Hz*/
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
