using System.Collections.Generic;

namespace NWaves.FeatureExtractors.Options
{
    public class LpcOptions : FeatureExtractorOptions
    {
        public int LpcOrder { get; set; }

        public override List<string> Errors
        {
            get
            {
                var errors = base.Errors;
                if (LpcOrder <= 1) errors.Add("LPC order must be greater than 1");
                return errors;
            }
        }
    }
}
