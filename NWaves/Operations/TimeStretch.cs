using System;
using NWaves.Filters.Base;
using NWaves.Operations.Tsm;
using NWaves.Signals;

namespace NWaves.Operations
{
    public static partial class Operation
    {
        /// <summary>
        /// Time stretching
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="stretch">Stretch factor (scale)</param>
        /// <param name="fftSize">Size of FFT</param>
        /// <param name="hopSize">Hop size</param>
        /// <param name="algorithm">Algorithm for TSM</param>
        /// <returns></returns>
        public static DiscreteSignal TimeStretch(DiscreteSignal signal,
                                                 double stretch,
                                                 int fftSize = 1024,
                                                 int hopSize = -1,
                                                 TsmAlgorithm algorithm = TsmAlgorithm.Wsola)
        {
            if (Math.Abs(stretch - 1.0) < 1e-10)
            {
                return signal.Copy();
            }

            var hopAnalysis = hopSize > 0 ? hopSize : fftSize / 4;
            
            IFilter stretchFilter;

            switch (algorithm)
            {
                case TsmAlgorithm.PhaseVocoder:
                    stretchFilter = new PhaseVocoder(stretch, hopAnalysis, fftSize);
                    break;
                default:
                    stretchFilter = new Wsola(stretch, fftSize);
                    break;
            }

            return stretchFilter.ApplyTo(signal, FilteringOptions.Auto);
        }
    }
}
