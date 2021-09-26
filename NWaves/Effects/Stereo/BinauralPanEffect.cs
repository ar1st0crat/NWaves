using NWaves.Filters.Base;
using NWaves.Operations.Convolution;
using NWaves.Utils;
using System;
using System.Linq;

namespace NWaves.Effects.Stereo
{
    /// <summary>
    /// Class representing binaural panning audio effect 
    /// (HRIR/BRIR interpolation + crossover filter (optional)).
    /// </summary>
    public class BinauralPanEffect : StereoEffect
    {
        /// <summary>
        /// Left ear HRIR.
        /// </summary>
        private readonly float[] _leftEarHrir;

        /// <summary>
        /// Right ear HRIR.
        /// </summary>
        private readonly float[] _rightEarHrir;

        /// <summary>
        /// HRIRs table (left ear).
        /// </summary>
        private readonly float[][][] _leftHrirTable;

        /// <summary>
        /// HRIRs table (right ear).
        /// </summary>
        private readonly float[][][] _rightHrirTable;

        /// <summary>
        /// Azimuths (thetas).
        /// </summary>
        private readonly float[] _azimuths;

        /// <summary>
        /// Elevations (phis).
        /// </summary>
        private readonly float[] _elevations;

        /// <summary>
        /// Left ear HRIR convolver.
        /// </summary>
        private readonly OlsBlockConvolver _leftEarConvolver;

        /// <summary>
        /// Right ear HRIR convolver.
        /// </summary>
        private readonly OlsBlockConvolver _rightEarConvolver;

        /// <summary>
        /// Turn on/off crossover filtering.
        /// </summary>
        private bool _useCrossover;

        /// <summary>
        /// Crossover filters (low-pass part).
        /// </summary>
        private IOnlineFilter _crossoverLpFilterLeft, _crossoverLpFilterRight;
        
        /// <summary>
        /// Crossover filters (high-pass part).
        /// </summary>
        private IOnlineFilter _crossoverHpFilterLeft, _crossoverHpFilterRight;

        /// <summary>
        /// Gets or sets azimuth.
        /// </summary>
        public float Azimuth
        {
            get => _azimuth;
            set
            {
                _azimuth = value;
                UpdateHrir(_azimuth, _elevation);
            }
        }
        private float _azimuth;

        /// <summary>
        /// Gets or sets elevation.
        /// </summary>
        public float Elevation
        {
            get => _elevation;
            set
            {
                _elevation = value;
                UpdateHrir(_azimuth, _elevation);
            }
        }
        private float _elevation;

        /// <summary>
        /// <para>Construct <see cref="BinauralPanEffect"/>.</para>
        /// <para>
        /// For example (CIPIC):
        /// <code>
        ///   25 azimuths (theta): <br/>
        ///     [-80 -65 -55 -45 -40 -35 -30 -25 -20 -15 -10 -5 0 5 10 15 20 25 30 35 40 45 55 65 80]  <br/>
        /// <br/> 
        ///   50 elevations (phi): <br/>
        ///     [-45 -39 -34 -28 -23 -17 -11 -6 0 6 11 17 23 28 34 39 45 51 56 62 68 73 79 84 90 96  <br/>
        ///       101 107 113 118 124 129 135 141 146 152 158 163 169 174 180 186 191 197 203 208 214 219 225 231] <br/>
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="azimuths">Azimuths (thetas) - must be sorted in ascending order</param>
        /// <param name="elevations">Elevations (phis) - must be sorted in ascending order</param>
        /// <param name="leftHrirs">HRIR collection (left ear)</param>
        /// <param name="rightHrirs">HRIR collection (right ear)</param>
        public BinauralPanEffect(float[] azimuths,
                                 float[] elevations,
                                 float[][][] leftHrirs,
                                 float[][][] rightHrirs)
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

            // crossover freq = 0.01 by default: e.g. 160 Hz with 16kHz sampling rate
            // (by default, crossover filtering is turned off)

            _crossoverLpFilterLeft = new Filters.BiQuad.LowPassFilter(0.01); 
            _crossoverHpFilterLeft = new Filters.BiQuad.HighPassFilter(0.01);
            _crossoverLpFilterRight = new Filters.BiQuad.LowPassFilter(0.01);
            _crossoverHpFilterRight = new Filters.BiQuad.HighPassFilter(0.01);

            // set initial azimuth = 0 and elevation = 0

            UpdateHrir(0, 0);
        }

        /// <summary>
        /// Turn on/off crossover filtering.
        /// </summary>
        public void UseCrossover(bool useCrossover)
        {
            _useCrossover = useCrossover;
        }

        /// <summary>
        /// Update frequency of the crossover filter (works only for BiQuadFilters). 
        /// Filters of other types / parameters can be passed to constructor.
        /// </summary>
        /// <param name="freq">Frequency</param>
        /// <param name="samplingRate">Sampling rate</param>
        public void SetCrossoverParameters(double freq, int samplingRate)
        {
            if (_crossoverLpFilterLeft is Filters.BiQuad.LowPassFilter lpFilterLeft)
            {
                lpFilterLeft.Change(freq / samplingRate);
            }

            if (_crossoverLpFilterRight is Filters.BiQuad.LowPassFilter lpFilterRight)
            {
                lpFilterRight.Change(freq / samplingRate);
            }

            if (_crossoverHpFilterLeft is Filters.BiQuad.HighPassFilter hpFilterLeft)
            {
                hpFilterLeft.Change(freq / samplingRate);
            }

            if (_crossoverHpFilterRight is Filters.BiQuad.HighPassFilter hpFilterRight)
            {
                hpFilterRight.Change(freq / samplingRate);
            }
        }

        /// <summary>
        /// Set custom crossover filters.
        /// </summary>
        /// <param name="lowpassLeft">Crossover filter (low-pass part) for left channel</param>
        /// <param name="highpassLeft">Crossover filter (high-pass part) for left channel</param>
        /// <param name="lowpassRight">Crossover filter (low-pass part) for right channel</param>
        /// <param name="highpassRight">Crossover filter (high-pass part) for right channel</param>
        public void SetCrossoverFilters(IOnlineFilter lowpassLeft,
                                        IOnlineFilter highpassLeft,
                                        IOnlineFilter lowpassRight,
                                        IOnlineFilter highpassRight)
        {
            _crossoverLpFilterLeft = lowpassLeft;
            _crossoverHpFilterLeft = highpassLeft;
            _crossoverLpFilterRight = lowpassRight;
            _crossoverHpFilterRight = highpassRight;
        }

        /// <summary>
        /// Update HRIR (interpolate it using HRIR tables).
        /// </summary>
        /// <param name="azimuth">Azimuth (theta)</param>
        /// <param name="elevation">Elevation (phi)</param>
        protected void UpdateHrir(float azimuth, float elevation)
        {
            // find 3 nearest points:

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
        /// Process one sample in each of two channels : [ input left , input right ] -> [ output left , output right ].
        /// </summary>
        /// <param name="left">Input sample in left channel</param>
        /// <param name="right">Input sample in right channel</param>
        public override void Process(ref float left, ref float right)
        {
            var leftIn = left;
            var rightIn = right;


            // 1) optional crossover filtering:

            var lowFreqSignalLeft = 0f;
            var lowFreqSignalRight = 0f;

            if (_useCrossover)
            {
                // extract low-frequency signal from source signal:

                lowFreqSignalLeft = _crossoverLpFilterLeft.Process(left);
                lowFreqSignalRight = _crossoverLpFilterRight.Process(right);

                // crossover highpass filtering
                // extract components that will be convolved with HRIRs: 

                left = _crossoverHpFilterLeft.Process(left);
                right = _crossoverHpFilterRight.Process(right);
            }
            
            
            // 2) HRIR filtering

            left = _leftEarConvolver.Process(left);
            right = _rightEarConvolver.Process(right);

            left += lowFreqSignalLeft;      // if there was no crossover filtering,
            right += lowFreqSignalRight;    // here we'll simply add zeros

            
            // 3) Wet/dry mixing (if necessary)

            left = leftIn * Dry + left * Wet;
            right = rightIn * Dry + right * Wet;
        }

        /// <summary>
        /// Reset binaural pan effect.
        /// </summary>
        public override void Reset()
        {
            _leftEarConvolver.Reset();
            _rightEarConvolver.Reset();
            _crossoverLpFilterLeft?.Reset();
            _crossoverLpFilterRight?.Reset();
            _crossoverHpFilterLeft?.Reset();
            _crossoverHpFilterRight?.Reset();
        }
    }
}
