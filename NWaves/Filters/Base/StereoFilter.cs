using NWaves.Signals;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Represents filter for processing data in interleaved stereo buffers. 
    /// <see cref="StereoFilter"/> is wrapped around two separate filters: 
    /// filter for signal in left channel and filter for signal in right channel.
    /// </summary>
    public class StereoFilter : IFilter, IOnlineFilter
    {
        /// <summary>
        /// Filter for signal in left channel.
        /// </summary>
        private readonly IOnlineFilter _filterLeft;

        /// <summary>
        /// Filter for signal in right channel.
        /// </summary>
        private readonly IOnlineFilter _filterRight;

        /// <summary>
        /// Internal flag for switching between left and right channels.
        /// </summary>
        private bool _isRight;

        /// <summary>
        /// Constructs <see cref="StereoFilter"/> from two separate filters.
        /// </summary>
        /// <param name="filterLeft">Filter for signal in left channel</param>
        /// <param name="filterRight">Filter for signal in right channel</param>
        public StereoFilter(IOnlineFilter filterLeft, IOnlineFilter filterRight)
        {
            _filterLeft = filterLeft;
            _filterRight = filterRight;
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public float Process(float sample)
        {
            if (_isRight)
            {
                _isRight = false;
                return _filterRight.Process(sample);
            }
            else
            {
                _isRight = true;
                return _filterLeft.Process(sample);
            }
        }

        /// <summary>
        /// Resets filters.
        /// </summary>
        public void Reset()
        {
            _filterLeft.Reset();
            _filterRight.Reset();
        }

        /// <summary>
        /// Processes entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="method">Filtering method</param>
        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto) => this.FilterOnline(signal);
    }
}
