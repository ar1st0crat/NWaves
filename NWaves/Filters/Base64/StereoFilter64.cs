using NWaves.Filters.Base;

namespace NWaves.Filters.Base64
{
    /// <summary>
    /// Represents filter for processing data in interleaved stereo buffers. 
    /// <see cref="StereoFilter64"/> is wrapped around two separate filters: 
    /// filter for signal in left channel and filter for signal in right channel.
    /// </summary>
    public class StereoFilter64 : IFilter64, IOnlineFilter64
    {
        /// <summary>
        /// Filter for signal in left channel.
        /// </summary>
        private readonly IOnlineFilter64 _filterLeft;

        /// <summary>
        /// Filter for signal in right channel.
        /// </summary>
        private readonly IOnlineFilter64 _filterRight;

        /// <summary>
        /// Internal flag for switching between left and right channels.
        /// </summary>
        private bool _isRight;

        /// <summary>
        /// Constructs <see cref="StereoFilter64"/> from two separate filters.
        /// </summary>
        /// <param name="filterLeft">Filter for signal in left channel</param>
        /// <param name="filterRight">Filter for signal in right channel</param>
        public StereoFilter64(IOnlineFilter64 filterLeft, IOnlineFilter64 filterRight)
        {
            _filterLeft = filterLeft;
            _filterRight = filterRight;
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public double Process(double sample)
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
        /// Applies filter to entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="method">Filtering method</param>
        public double[] ApplyTo(double[] signal, FilteringMethod method = FilteringMethod.Auto) => this.FilterOnline(signal);
    }
}
