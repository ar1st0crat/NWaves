using System;

namespace NWaves.DemoXamarin.DependencyServices
{
    public class PitchEstimatedEventArgs : EventArgs
    {
        public float PitchZcr { get; set; }
        public float PitchAutoCorr { get; set; }
    }
}
