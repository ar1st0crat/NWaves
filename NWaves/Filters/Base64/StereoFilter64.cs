using NWaves.Filters.Base;
using System.Linq;

namespace NWaves.Filters.Base64
{
    /// <summary>
    /// Filter for filtering data in interleaved stereo buffers (double precision)
    /// </summary>
    public class StereoFilter64 : IFilter64, IOnlineFilter64
    {
        /// <summary>
        /// Filter for signal in left channel
        /// </summary>
        private readonly IOnlineFilter64 _filterLeft;

        /// <summary>
        /// Filter for signal in right channel
        /// </summary>
        private readonly IOnlineFilter64 _filterRight;

        /// <summary>
        /// Internal flag for switching between left and right channels
        /// </summary>
        private bool _isRight;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filterLeft"></param>
        /// <param name="filterRight"></param>
        public StereoFilter64(IOnlineFilter64 filterLeft, IOnlineFilter64 filterRight)
        {
            _filterLeft = filterLeft;
            _filterRight = filterRight;
        }

        /// <summary>
        /// Online filtering
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public double Process(double input)
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
        public double[] ApplyTo(double[] signal, FilteringMethod method = FilteringMethod.Auto)
        {
            return signal.Select(s => Process(s)).ToArray();
        }
    }
}
