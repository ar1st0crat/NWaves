namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Wave table builder
    /// </summary>
    public class WaveTableBuilder : SignalBuilder
    {
        private readonly float[] _samples;

        public WaveTableBuilder(float[] samples)
        {
            _samples = samples;
        }

        public override float NextSample()
        {
            if (_n == _samples.Length)
            {
                _n = 0;
            }

            return _samples[_n++];
        }

        public override void Reset()
        {
            _n = 0;
        }

        private int _n;
    }
}
