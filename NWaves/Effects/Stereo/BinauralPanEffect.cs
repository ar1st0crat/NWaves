using NWaves.Filters.Base;
using NWaves.Operations.Convolution;
using NWaves.Utils;
using System;
using System.Linq;

namespace NWaves.Effects.Stereo
{
    /// <summary>
    /// Binaural panning (HRTF/HRIR + BRIR (optional) + high-pass crossover filter (optional))
    /// </summary>
    public class BinauralPanEffect : StereoEffect
    {
        /// <summary>
        /// Left ear HRIR
        /// </summary>
        private float[] _leftEarHrir;

        /// <summary>
        /// Right ear HRIR
        /// </summary>
        private float[] _rightEarHrir;

        /// <summary>
        /// HRIRs table (left ear)
        /// </summary>
        private readonly float[][][] _leftHrirTable;

        /// <summary>
        /// HRIRs table (right ear)
        /// </summary>
        private readonly float[][][] _rightHrirTable;

        /// <summary>
        /// Azimuths (thetas)
        /// </summary>
        private readonly float[] _azimuths;

        /// <summary>
        /// Elevations (phis)
        /// </summary>
        private readonly float[] _elevations;

        /// <summary>
        /// Room impulse reponse (BRIR (binaural) / SRIR (spatial) / DRIR (directional))
        /// </summary>
        private readonly float[] _rir;

        /// <summary>
        /// Left ear HRIR convolver
        /// </summary>
        private readonly OlsBlockConvolver _leftEarConvolver;

        /// <summary>
        /// Right ear HRIR convolver
        /// </summary>
        private readonly OlsBlockConvolver _rightEarConvolver;

        /// <summary>
        /// Room impulse response convolver
        /// </summary>
        private readonly OlsBlockConvolver _rirConvolver;

        /// <summary>
        /// Crossover (lowpass) filter
        /// </summary>
        private IOnlineFilter _crossoverFilter;


        /// <summary>
        /// Constructor
        /// 
        /// For example, CIPIC:
        /// 
        ///   25 azimuths (theta):
        ///     [-80 -65 -55 -45 -40 -35 -30 -25 -20 -15 -10 -5 0 5 10 15 20 25 30 35 40 45 55 65 80]  
        /// 
        ///   50 elevations (phi):
        ///     [-45 -39 -34 -28 -23 -17 -11 -6 0 6 11 17 23 28 34 39 45 51 56 62 68 73 79 84 90 96 
        ///       101 107 113 118 124 129 135 141 146 152 158 163 169 174 180 186 191 197 203 208 214 219 225 231]
        /// 
        /// </summary>
        /// <param name="azimuths">Azimuths (thetas) - must be sorted in ascending order</param>
        /// <param name="elevations">Elevations (phis) - must be sorted in ascending order</param>
        /// <param name="leftHrirs">HRIR collection (left ear)</param>
        /// <param name="rightHrirs">HRIR collection (right ear)</param>
        /// <param name="rir">Room impulse response</param>
        /// <param name="crossoverFilter">Crossover filter</param>
        public BinauralPanEffect(float[] azimuths,
                                 float[] elevations,
                                 float[][][] leftHrirs,
                                 float[][][] rightHrirs,
                                 float[] rir = null,
                                 IOnlineFilter crossoverFilter = null)
        {
            _azimuths = azimuths ?? throw new ArgumentNullException(nameof(azimuths));
            _elevations = elevations ?? throw new ArgumentNullException(nameof(elevations));
            _leftHrirTable = leftHrirs ?? throw new ArgumentNullException(nameof(leftHrirs));
            _rightHrirTable = rightHrirs ?? throw new ArgumentNullException(nameof(rightHrirs));

            if (leftHrirs.Any(h => h is null) || 
                rightHrirs.Any(h => h is null))
            {
                throw new ArgumentNullException("One of HRIRs is null!");
            }

            if (leftHrirs.Any(h => h.Any(r => r is null)) ||
                rightHrirs.Any(h => h.Any(r => r is null)))
            {
                throw new ArgumentNullException("One of HRIRs is null!");
            }

            var hrirLength = leftHrirs[0][0].Length;

            if (leftHrirs.Any(h => h.Any(r => r.Length != hrirLength)) || 
                rightHrirs.Any(h => h.Any(r => r.Length != hrirLength)))
            {
                throw new ArgumentException("All HRIRs must have the same size!");
            }

            _leftEarHrir = new float[hrirLength];
            _rightEarHrir = new float[hrirLength];

            _leftEarConvolver = new OlsBlockConvolver(_leftEarHrir, MathUtils.NextPowerOfTwo(4 * hrirLength));
            _rightEarConvolver = new OlsBlockConvolver(_rightEarHrir, MathUtils.NextPowerOfTwo(4 * hrirLength));

            _crossoverFilter = crossoverFilter;

            if (rir != null)
            {
                _rir = rir;
                _rirConvolver = new OlsBlockConvolver(rir, MathUtils.NextPowerOfTwo(4 * _rir.Length));
            }

            UpdateHrir(0, 0);
        }

        /// <summary>
        /// Set frequency of the crossover filter
        /// </summary>
        /// <param name="freq">Frequency</param>
        /// <param name="samplingRate">Sampling rate</param>
        public void SetCrossoverParameters(double freq, int samplingRate)
        {
            if (_crossoverFilter is Filters.BiQuad.HighPassFilter filter)
            {
                filter.Change(freq / samplingRate);
            }
            else
            {
                _crossoverFilter = new Filters.BiQuad.HighPassFilter(freq / samplingRate);
            }
        }

        /// <summary>
        /// Update HRIR (interpolate it using HRIR tables)
        /// </summary>
        /// <param name="azimuth">Azimuth (theta)</param>
        /// <param name="elevation">Elevation (phi)</param>
        protected void UpdateHrir(float azimuth, float elevation)
        {
            //float HaversineDistance(float az1, float el1, float az2, float el2)
            //{
            //    var elSin = Math.Sin((el2 - el1) / 2) * Math.Sin((el2 - el1) / 2);
            //    var azSin = Math.Sin((az2 - az1) / 2) * Math.Sin((az2 - az1) / 2);

            //    return (float)Math.Acos(Math.Sqrt(elSin + azSin * Math.Cos(el1) * Math.Cos(el2)));
            //}

            // find 3 closest points:

            var bestAzimuthPos = 0;
            var bestElevationPos = 0;

            foreach (var az in _azimuths)
            {
                if (az >= azimuth)
                {
                    break;
                }
                bestAzimuthPos++;
            }

            foreach (var el in _elevations)
            {
                if (el >= elevation)
                {
                    break;
                }
                bestElevationPos++;
            }

            var secondAzimuthPos = bestAzimuthPos - 1;
            var secondElevationPos = bestElevationPos - 1;

            // first boundary case (don't do actual interpolation)

            if (secondAzimuthPos < 0 || secondElevationPos < 0)
            {
                _leftHrirTable[bestAzimuthPos][bestElevationPos].FastCopyTo(_leftEarHrir, _leftEarHrir.Length);
                _rightHrirTable[bestAzimuthPos][bestElevationPos].FastCopyTo(_rightEarHrir, _rightEarHrir.Length);
            }

            // second boundary case (don't do actual interpolation)

            else if (bestAzimuthPos == _azimuths.Length || bestElevationPos == _elevations.Length)
            {
                _leftHrirTable[secondAzimuthPos][secondElevationPos].FastCopyTo(_leftEarHrir, _leftEarHrir.Length);
                _rightHrirTable[secondAzimuthPos][secondElevationPos].FastCopyTo(_rightEarHrir, _rightEarHrir.Length);
            }

            // normal case (do actual interpolation)

            else
            {
                // perhaps, swap 'best' and 'second' positions

                if (_azimuths[bestAzimuthPos] - azimuth > azimuth - _azimuths[secondAzimuthPos])
                {
                    var tmp = bestAzimuthPos;
                    bestAzimuthPos = secondAzimuthPos;
                    secondAzimuthPos = tmp;
                }

                if (_elevations[bestElevationPos] - elevation > elevation - _elevations[secondElevationPos])
                {
                    var tmp = bestElevationPos;
                    bestElevationPos = secondElevationPos;
                    secondElevationPos = tmp;
                }

                // barycentric interpolation points:
                //
                //    p1            p2            p3
                //    [az1, el1]    [az1, el2]    [az2, el1]
                //

                var az1 = _azimuths[bestAzimuthPos];
                var az2 = _azimuths[secondAzimuthPos];
                var el1 = _elevations[bestElevationPos];
                var el2 = _elevations[secondElevationPos];
                
                var alpha = ((el2 - el1) * (azimuth - az2) + (az2 - az1) * (elevation - el1)) / ((el2 - el1) * (az1 - az2));
                var beta = (az1 - az2) * (elevation - el1) / ((el2 - el1) * (az1 - az2));
                var gamma = 1 - alpha - beta;

                var p1LTable = _leftHrirTable[bestAzimuthPos][bestElevationPos];
                var p2LTable = _leftHrirTable[bestAzimuthPos][secondElevationPos];
                var p3LTable = _leftHrirTable[secondAzimuthPos][bestElevationPos];
                var p1RTable = _rightHrirTable[bestAzimuthPos][bestElevationPos];
                var p2RTable = _rightHrirTable[bestAzimuthPos][secondElevationPos];
                var p3RTable = _rightHrirTable[secondAzimuthPos][bestElevationPos];

                for (var i = 0; i < _leftEarHrir.Length; i++)
                {
                    _leftEarHrir[i] = alpha * p1LTable[i] + beta * p2LTable[i] + gamma * p3LTable[i];
                    _rightEarHrir[i] = alpha * p1RTable[i] + beta * p2RTable[i] + gamma * p3RTable[i];
                }
            }

            _leftEarConvolver.ChangeKernel(_leftEarHrir);
            _rightEarConvolver.ChangeKernel(_rightEarHrir);
        }

        /// <summary>
        /// Process current sample in each channel
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public override void Process(ref float left, ref float right)
        {
            if (_crossoverFilter != null)
            {
                left = _crossoverFilter.Process(left);      // crossover highpass filtering
                right = _crossoverFilter.Process(right);
            }

            left = _leftEarConvolver.Process(left);         // HRIR filtering
            right = _leftEarConvolver.Process(right);

            if (_rirConvolver != null)                      // room reverb
            {
                left = _rirConvolver.Process(left);
                right = _rirConvolver.Process(right);
            }
        }

        /// <summary>
        /// Reset binaural pan effect
        /// </summary>
        public override void Reset()
        {
            _leftEarConvolver.Reset();
            _rightEarConvolver.Reset();
            _rirConvolver.Reset();
            _crossoverFilter.Reset();
        }
    }
}
