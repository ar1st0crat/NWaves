using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Class representing Infinite Impulse Response filters
    /// </summary>
    public class IirFilter : LtiFilter
    {
        /// <summary>
        /// Denominator part coefficients in filter's transfer function 
        /// (recursive part in difference equations)
        /// </summary>
        protected double[] _a;
        protected double[] A
        {
            get
            {
                return _a;
            }
            set
            {
                _a = value;
                _a32 = _a.ToFloats();
            }
        }

        /// <summary>
        /// Float versions of denominator coefficients for filtering (by default)
        /// </summary>
        protected float[] _a32;

        /// <summary>
        /// Numerator part coefficients in filter's transfer function 
        /// (non-recursive part in difference equations)
        /// </summary>
        protected double[] _b;
        protected double[] B
        {
            get
            {
                return _b;
            }
            set
            {
                _b = value;
                _b32 = _b.ToFloats();
            }
        }

        /// <summary>
        /// Transfer function (created lazily or set specifically if needed)
        /// </summary>
        private TransferFunction _tf;
        public override TransferFunction Tf
        {
            get => _tf ?? new TransferFunction(_b, _a);
            protected set => _tf = value;
        }

        /// <summary>
        /// Float versions of numerator coefficients for filtering (by default)
        /// </summary>
        protected float[] _b32;
        
        /// <summary>
        /// Default length of truncated impulse response
        /// </summary>
        public const int DefaultImpulseResponseLength = 512;

        /// <summary>
        /// Internal buffers for delay lines
        /// </summary>
        protected float[] _delayLineB;
        protected float[] _delayLineA;

        /// <summary>
        /// Current offsets in delay lines
        /// </summary>
        protected int _delayLineOffsetB;
        protected int _delayLineOffsetA;

        /// <summary>
        /// Parameterized constructor (from arrays of coefficients)
        /// </summary>
        /// <param name="b">TF numerator coefficients</param>
        /// <param name="a">TF denominator coefficients</param>
        public IirFilter(IEnumerable<double> b, IEnumerable<double> a)
        {
            B = b.ToArray();
            A = a.ToArray();
            ResetInternals();
        }

        /// <summary>
        /// Parameterized constructor (from transfer function)
        /// </summary>
        /// <param name="tf">Transfer function</param>
        public IirFilter(TransferFunction tf)
        {
            B = tf.Numerator;
            A = tf.Denominator;
            Tf = tf;
            ResetInternals();
        }

        /// <summary>
        /// Apply filter to entire signal (offline)
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public override DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringMethod method = FilteringMethod.Auto)
        {
            switch (method)
            {
                case FilteringMethod.Custom:
                {
                    return this.ProcessChunks(signal);
                }
                case FilteringMethod.OverlapAdd:       // are you sure you wanna do this? It's IIR filter!
                case FilteringMethod.OverlapSave:
                {
                    var length = Math.Max(DefaultImpulseResponseLength, _a.Length + _b.Length);
                    var fftSize = MathUtils.NextPowerOfTwo(4 * length);
                    var ir = new DiscreteSignal(signal.SamplingRate, ImpulseResponse(length).ToFloats());
                    return Operation.BlockConvolve(signal, ir, fftSize, method);
                }
                default:
                {
                    return ApplyFilterDirectly(signal);
                }
            }
        }

        /// <summary>
        /// IIR online filtering (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var a = _a32;
            var b = _b32;

            var output = 0.0f;

            _delayLineB[_delayLineOffsetB] = sample;

            var pos = 0;
            for (var k = _delayLineOffsetB; k < b.Length; k++)
            {
                output += b[pos++] * _delayLineB[k];
            }
            for (var k = 0; k < _delayLineOffsetB; k++)
            {
                output += b[pos++] * _delayLineB[k];
            }

            pos = 1;
            for (var p = _delayLineOffsetA + 1; p < a.Length; p++)
            {
                output -= a[pos++] * _delayLineA[p];
            }
            for (var p = 0; p < _delayLineOffsetA; p++)
            {
                output -= a[pos++] * _delayLineA[p];
            }

            _delayLineA[_delayLineOffsetA] = output;

            if (--_delayLineOffsetB < 0)
            {
                _delayLineOffsetB = _delayLineB.Length - 1;
            }

            if (--_delayLineOffsetA < 0)
            {
                _delayLineOffsetA = _delayLineA.Length - 1;
            }

            return output;
        }

        /// <summary>
        /// The most straightforward implementation of the difference equation:
        /// code the difference equation as it is
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyFilterDirectly(DiscreteSignal signal)
        {
            var input = signal.Samples;
            var a = _a32;
            var b = _b32;

            var output = new float[input.Length];

            for (var n = 0; n < input.Length; n++)
            {
                for (var k = 0; k < b.Length; k++)
                {
                    if (n >= k) output[n] += b[k] * input[n - k];
                }
                for (var m = 1; m < a.Length; m++)
                {
                    if (n >= m) output[n] -= a[m] * output[n - m];
                }
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Reset internal buffers
        /// </summary>
        protected void ResetInternals()
        {
            if (_delayLineB == null || _delayLineA == null)
            {
                _delayLineB = new float[_b.Length];
                _delayLineA = new float[_a.Length];
            }
            else
            {
                for (var i = 0; i < _delayLineB.Length; i++)
                {
                    _delayLineB[i] = 0;
                }
                for (var i = 0; i < _delayLineA.Length; i++)
                {
                    _delayLineA[i] = 0;
                }
            }

            _delayLineOffsetB = _delayLineB.Length - 1;
            _delayLineOffsetA = _delayLineA.Length - 1;
        }

        /// <summary>
        /// Reset filter
        /// </summary>
        public override void Reset()
        {
            ResetInternals();
        }

        /// <summary>
        /// Divide all filter coefficients by A[0]
        /// </summary>
        public void Normalize()
        {
            var first = A[0];

            if (Math.Abs(first - 1.0) < 1e-10)
            {
                return;
            }

            if (Math.Abs(first) < 1e-10)
            {
                throw new ArgumentException("The first A coefficient can not be zero!");
            }
            
            for (var i = 0; i < A.Length; i++)
            {
                A[i] /= first;
                _a32[i] = (float) A[i];
            }

            for (var i = 0; i < B.Length; i++)
            {
                B[i] /= first;
                _b32[i] = (float) B[i];
            }
        }

        /// <summary>
        /// Returns the real-valued impulse response of a filter.
        /// 
        /// Method calculates the Impulse Response of a filter
        /// by feeding the unit impulse into it.
        /// </summary>
        /// <param name="length">
        /// The length of an impulse reponse.
        /// It's the length of truncated infinite impulse reponse.
        /// </param>
        public override double[] ImpulseResponse(int length = DefaultImpulseResponseLength)
        {
            var response = new double[length];
            var impulse = new double[length];
            impulse[0] = 1.0;

            ApplyTo(impulse, response);

            return response;
        }

        /// <summary>
        /// Sequential combination of two IIR filters
        /// </summary>
        /// <param name="filter1"></param>
        /// <param name="filter2"></param>
        /// <returns></returns>
        public static IirFilter operator *(IirFilter filter1, IirFilter filter2)
        {
            var tf = filter1.Tf * filter2.Tf;
            return new IirFilter(tf.Numerator, tf.Denominator);
        }

        /// <summary>
        /// Parallel combination of an IIR and any LTI filter
        /// </summary>
        /// <param name="filter1"></param>
        /// <param name="filter2"></param>
        /// <returns></returns>
        public static IirFilter operator +(IirFilter filter1, LtiFilter filter2)
        {
            var tf = filter1.Tf + filter2.Tf;
            return new IirFilter(tf.Numerator, tf.Denominator);
        }

        #region double precision computations

        public void ApplyTo(double[] input, double[] output)
        {
            for (var n = 0; n < input.Length; n++)
            {
                for (var k = 0; k < _b.Length; k++)
                {
                    if (n >= k) output[n] += _b[k] * input[n - k];
                }
                for (var m = 1; m < _a.Length; m++)
                {
                    if (n >= m) output[n] -= _a[m] * output[n - m];
                }
            }
        }

        #endregion
    }
}
