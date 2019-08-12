using System;

namespace NWaves.Operations.Tsm
{
    /// <summary>
    /// TSM processor based on Paul stretch algorithm
    /// </summary>
    class PaulStretch : PhaseVocoder
    {
        /// <summary>
        /// Randomizer for phases
        /// </summary>
        private readonly Random _rand = new Random();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stretch"></param>
        /// <param name="hopAnalysis"></param>
        /// <param name="fftSize"></param>
        public PaulStretch(double stretch, int hopAnalysis, int fftSize = 0)
            : base(stretch, hopAnalysis, fftSize)
        {
        }

        /// <summary>
        /// Process spectrum at each STFT step: simply randomize phases
        /// </summary>
        public override void ProcessSpectrum()
        {
            for (var j = 1; j <= _fftSize / 2; j++)
            {
                var mag = Math.Sqrt(_re[j] * _re[j] + _im[j] * _im[j]);
                var phase = 2 * Math.PI * _rand.NextDouble();

                _re[j] = (float)(mag * Math.Cos(phase));
                _im[j] = (float)(mag * Math.Sin(phase));
            }
        }

        /// <summary>
        /// Reset (nothing to do here)
        /// </summary>
        public override void Reset()
        {
        }
    }
}
