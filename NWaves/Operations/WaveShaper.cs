using NWaves.Filters.Base;
using NWaves.Signals;
using System;

namespace NWaves.Operations
{
    /// <summary>
    /// <para>Represents wave shaper.</para>
    /// <para>
    /// Wave shaper is a filter that maps an input signal to the output signal 
    /// by applying arbitrary mathematical function (shaping function) to the input signal.
    /// </para>
    /// </summary>
    public class WaveShaper : IFilter, IOnlineFilter
    {
        private readonly Func<float, float> _waveShapingFunction;

        /// <summary>
        /// Constructs <see cref="WaveShaper"/> using <paramref name="waveShapingFunction"/>.
        /// </summary>
        /// <param name="waveShapingFunction">Wave shaping function</param>
        public WaveShaper(Func<float, float> waveShapingFunction)
        {
            _waveShapingFunction = waveShapingFunction;
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public float Process(float sample) => _waveShapingFunction(sample);

        /// <summary>
        /// Resets wave shaper.
        /// </summary>
        public void Reset()
        {
        }

        /// <summary>
        /// Processes entire <paramref name="signal"/> and returns new wave-shaped signal.
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="method">Filtering method</param>
        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto) => this.FilterOnline(signal);
    }
}
