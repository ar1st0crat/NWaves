using System;

namespace NWaves.Utils
{
    /// <summary>
    /// Fractional delay line
    /// </summary>
    public class FractionalDelayLine
    {
        public enum InterpolationMode
        {
            Linear,
            Lagrange,
            Nearest
        }

        /// <summary>
        /// Sampling rate
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Delay line
        /// </summary>
        private float[] _delayLine;
        private int _maxDelayPos;
        private int _n;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="maxDelay"></param>
        /// <param name="interpolationMode"></param>
        public FractionalDelayLine(int samplingRate, double maxDelay, InterpolationMode interpolationMode = InterpolationMode.Linear)
        {
            _fs = samplingRate;
            //_maxDelayPos = (int)Math.Ceiling(_fs * maxDelay);
            //_delayLine = new float[_maxDelayPos + 1];
            _maxDelayPos = (int)(_fs * maxDelay);
            _delayLine = new float[_maxDelayPos + 2];
            _n = 0;
        }

        /// <summary>
        /// Write (put) sample in the delay line
        /// </summary>
        /// <param name="sample"></param>
        public void Write(float sample)
        {
            _delayLine[_n] = sample;

            if (++_n >= _delayLine.Length)
            {
                _n = 0;
            }
        }

        /// <summary>
        /// Read (get) sample from the delay line corresponding to given time delay (in seconds)
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public float Read(double delay)
        {
            var readPosition = (float) (_n - 1 - delay + _delayLine.Length) % _delayLine.Length;
            
            var localReadPosition = (int)Math.Floor(readPosition);

            var fraction = readPosition - localReadPosition;
            var delayed1 = _delayLine[localReadPosition];
            var delayed2 = _delayLine[(localReadPosition + 1) % _delayLine.Length];

            return delayed1 + fraction * (delayed2 - delayed1);
        }

        /// <summary>
        /// Read (get) sample from the delay line by index
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public float Read(int pos) => _delayLine[pos];
    }
}
