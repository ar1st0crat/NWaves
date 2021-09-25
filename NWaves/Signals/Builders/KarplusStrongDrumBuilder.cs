using NWaves.Signals.Builders.Base;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for generating signals using the "Drum" variation of Karplus-Strong algorithm.
    /// <para>
    /// Parameters that can be set in method <see cref="SignalBuilder.SetParameter(string, double)"/>: 
    /// <list type="bullet">
    ///     <item>"frequency", "freq", "f" (default: 100.0 Hz)</item>
    ///     <item>"stretch", "s" (default: 1.0)</item>
    ///     <item>"feedback", "a" (default: 1.0)</item>
    ///     <item>"probability", "prob" (default: 0.5)</item>
    /// </list>
    /// </para>
    /// </summary>
    public class KarplusStrongDrumBuilder : KarplusStrongBuilder
    {
        private double _probability = 0.5;

        /// <summary>
        /// Constructs <see cref="KarplusStrongDrumBuilder"/>.
        /// </summary>
        public KarplusStrongDrumBuilder() : base(null)
        {
            Init();
        }

        /// <summary>
        /// Constructs <see cref="KarplusStrongDrumBuilder"/> from array of <paramref name="samples"/>.
        /// </summary>
        /// <param name="samples">Array of samples</param>
        public KarplusStrongDrumBuilder(float[] samples) : base(samples)
        {
            Init();
        }

        private void Init()
        {
            ParameterSetters.Add("probability, prob", param => _probability = param);
        }

        /// <summary>
        /// Generate new sample.
        /// </summary>
        public override float NextSample()
        {
            var idx = ((int)_n) % _samples.Length;

            if (_rand.NextDouble() < 1 / _stretchFactor)
            {
                if (_rand.NextDouble() < _probability)
                {
                    _samples[idx] = 0.5f * (_samples[idx] + _prev) * _feedback;
                }
                else
                {
                    _samples[idx] = -0.5f * (_samples[idx] + _prev) * _feedback;
                }
            }

            _prev = _samples[idx];
            _n++;

            return _prev;
        }
    }
}
