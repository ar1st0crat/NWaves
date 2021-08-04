using System;

namespace NWaves.Utils
{
    /// <summary>
    /// Fractional delay line
    /// </summary>
    public partial class FractionalDelayLine
    {
        /// <summary>
        /// Sampling rate
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Delay line
        /// </summary>
        private float[] _delayLine;
        private int _delayLineSize;
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

            _delayLine = new float[(int)(_fs * maxDelay) + 1]; // +2];
            _delayLineSize = _delayLine.Length;
            
            _n = 0;
        }

        /// <summary>
        /// Write (put) sample in the delay line
        /// </summary>
        /// <param name="sample"></param>
        public void Write(float sample)
        {
            _delayLine[_n] = sample;

            if (++_n >= _delayLineSize)
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
            //var precisePosition = (float)(_n - 1 - delay + _delayLineSize) % _delayLineSize;
            var precisePosition = (float) (_n - delay + _delayLineSize) % _delayLineSize;

            var intPosition = (int)precisePosition;  // (int)Math.Floor(precisePosition);

            var fraction = precisePosition - intPosition;
            var delayed1 = _delayLine[intPosition];
            var delayed2 = _delayLine[(intPosition + 1) % _delayLineSize];

            return delayed1 + fraction * (delayed2 - delayed1);
        }

        /// <summary>
        /// Read (get) sample from the delay line by index
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public float Read(int pos) => _delayLine[pos];

        /// <summary>
        /// Reset delay line
        /// </summary>
        public void Reset()
        {
            Array.Clear(_delayLine, 0, _delayLineSize);
            _n = 0;
        }
    }
}
