using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    [DataContract]
    public class LpcOptions : FeatureExtractorOptions
    {
        /// <summary>
        /// Order of LPC is required. It has priority over FeatureCount. 
        /// FeatureCount will be autocomputed as LpcOrder + 1.
        /// </summary>
        [DataMember]
        public int LpcOrder { get; set; }

        public override List<string> Errors
        {
            get
            {
                var errors = base.Errors;
                if (LpcOrder <= 0) errors.Add("Positive order of LPC must be specified");
                return errors;
            }
        }
    }
}
