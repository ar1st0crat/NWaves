using System;

namespace NWaves.DemoXamarin.DependencyServices
{
    public interface IAudioService
    {
        void StartRecording();
        void StopRecording();

        event EventHandler<PitchEstimatedEventArgs> PitchEstimated;
    }
}
