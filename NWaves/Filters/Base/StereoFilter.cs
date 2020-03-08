using NWaves.Signals;
using System.Linq;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Filter for filtering data in interleaved stereo buffers
    /// </summary>
    public class StereoFilter : IFilter, IOnlineFilter
    {
        /// <summary>
        /// Filter for signal in left channel
        /// </summary>
        private readonly IOnlineFilter _filterLeft;

        /// <summary>
        /// Filter for signal in right channel
        /// </summary>
        private readonly IOnlineFilter _filterRight;

        /// <summary>
        /// Internal flag for switching between left and right channels
        /// </summary>
        private bool _isRight;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filterLeft"></param>
        /// <param name="filterRight"></param>
        public StereoFilter(IOnlineFilter filterLeft, IOnlineFilter filterRight)
        {
            _filterLeft = filterLeft;
            _filterRight = filterRight;
        }

        /// <summary>
        /// Online filtering
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public float Process(float input)
        {
            if (_isRight)
            {
                _isRight = false;
                return _filterRight.Process(input);
            }
            else
            {
                _isRight = true;
                return _filterLeft.Process(input);
            }
        }

        /// <summary>
        /// Reset filters
        /// </summary>
        public void Reset()
        {
            _filterLeft.Reset();
            _filterRight.Reset();
        }

        /// <summary>
        /// Offline filtering
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)
        {
            return new DiscreteSignal(signal.SamplingRate, signal.Samples.Select(s => Process(s)));
        }
    }
}
