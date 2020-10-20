using NWaves.Filters.Base;
using NWaves.Operations.Convolution;
using NWaves.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Filters.Base64
{
    public class IirFilter64 : LtiFilter64
    {
        /// <summary>
        /// Numerator part coefficients in filter's transfer function 
        /// (non-recursive part in difference equations)
        /// 
        /// Note.
        /// This array is created from duplicated coefficients:
        /// 
        ///  numerator              _b
        /// [1 2 3 4 5] -> [1 2 3 4 5 1 2 3 4 5]
        /// 
        /// Such memory layout leads to speed-up of online filtering.
        /// </summary>
        protected readonly double[] _b;

        /// <summary>
        /// Denominator part coefficients in filter's transfer function 
        /// (recursive part in difference equations).
        /// 
        /// Note.
        /// This array is created from duplicated coefficients:
        /// 
        ///  denominator             _a
        ///  [1 2 3 4 5] -> [1 2 3 4 5 1 2 3 4 5]
        /// 
        /// Such memory layout leads to speed-up of online filtering.
        /// </summary>
        protected readonly double[] _a;

        /// <summary>
        /// Number of numerator coefficients
        /// </summary>
        protected readonly int _numeratorSize;

        /// <summary>
        /// Number of denominator (feedback) coefficients
        /// </summary>
        protected readonly int _denominatorSize;

        /// <summary>
        /// Transfer function (created lazily or set specifically if needed)
        /// </summary>
        protected TransferFunction _tf;
        public override TransferFunction Tf
        {
            get => _tf ?? new TransferFunction(_b.Take(_numeratorSize).ToArray(), _a.Take(_denominatorSize).ToArray());
            protected set => _tf = value;
        }

        /// <summary>
        /// Default length of truncated impulse response
        /// </summary>
        public int DefaultImpulseResponseLength { get; set; } = 512;

        /// <summary>
        /// Internal buffers for delay lines
        /// </summary>
        protected double[] _delayLineA;
        protected double[] _delayLineB;

        /// <summary>
        /// Current offsets in delay lines
        /// </summary>
        protected int _delayLineOffsetA;
        protected int _delayLineOffsetB;

        /// <summary>
        /// Parameterized constructor (from arrays of 64-bit coefficients)
        /// </summary>
        /// <param name="b">TF numerator coefficients</param>
        /// <param name="a">TF denominator coefficients</param>
        public IirFilter64(IEnumerable<double> b, IEnumerable<double> a)
        {
            _numeratorSize = b.Count();
            _denominatorSize = a.Count();

            _b = new double[_numeratorSize * 2];

            for (var i = 0; i < _numeratorSize; i++)
            {
                _b[i] = _b[_numeratorSize + i] = b.ElementAt(i);
            }

            _a = new double[_denominatorSize * 2];

            for (var i = 0; i < _denominatorSize; i++)
            {
                _a[i] = _a[_denominatorSize + i] = a.ElementAt(i);
            }

            _delayLineB = new double[_numeratorSize];
            _delayLineA = new double[_denominatorSize];
            _delayLineOffsetB = _numeratorSize - 1;
            _delayLineOffsetA = _denominatorSize - 1;
        }

        /// <summary>
        /// Parameterized constructor (from transfer function).
        /// 
        /// Coefficients (used for filtering) will be cast to doubles anyway,
        /// but filter will store the reference to TransferFunction object for FDA.
        /// 
        /// </summary>
        /// <param name="tf">Transfer function</param>
        public IirFilter64(TransferFunction tf) : this(tf.Numerator, tf.Denominator)
        {
            Tf = tf;
        }

        /// <summary>
        /// Apply filter to entire signal (offline)
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public override double[] ApplyTo(double[] signal, FilteringMethod method = FilteringMethod.Auto)
        {
            switch (method)
            {
                case FilteringMethod.OverlapAdd:       // are you sure you wanna do this? It's IIR filter!
                case FilteringMethod.OverlapSave:
                    {
                        var length = Math.Max(DefaultImpulseResponseLength, _denominatorSize + _numeratorSize);
                        var fftSize = MathUtils.NextPowerOfTwo(4 * length);
                        var ir = Tf.ImpulseResponse(length);
                        return new OlsBlockConvolver64(ir, fftSize).ApplyTo(signal);
                    }
                default:
                    {
                        return signal.Select(s => Process(s)).ToArray();
                    }
            }
        }

        /// <summary>
        /// IIR online filtering (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override double Process(double sample)
        {
            var output = 0.0;

            _delayLineB[_delayLineOffsetB] = sample;
            _delayLineA[_delayLineOffsetA] = 0;

            for (int i = 0, j = _numeratorSize - _delayLineOffsetB; i < _numeratorSize; i++, j++)
            {
                output += _delayLineB[i] * _b[j];
            }

            for (int i = 0, j = _denominatorSize - _delayLineOffsetA; i < _denominatorSize; i++, j++)
            {
                output -= _delayLineA[i] * _a[j];
            }

            _delayLineA[_delayLineOffsetA] = output;

            if (--_delayLineOffsetB < 0)
            {
                _delayLineOffsetB = _numeratorSize - 1;
            }

            if (--_delayLineOffsetA < 0)
            {
                _delayLineOffsetA = _denominatorSize - 1;
            }

            return output;
        }

        /// <summary>
        /// Change filter coefficients online (numerator part)
        /// </summary>
        /// <param name="b">New coefficients</param>
        public void ChangeNumeratorCoeffs(double[] b)
        {
            if (b.Length == _numeratorSize)
            {
                for (var i = 0; i < _numeratorSize; i++)
                {
                    _b[i] = _b[_numeratorSize + i] = b[i];
                }
            }
        }

        /// <summary>
        /// Change filter coefficients online (denominator / recursive part)
        /// </summary>
        /// <param name="a">New coefficients</param>
        public void ChangeDenominatorCoeffs(double[] a)
        {
            if (a.Length == _denominatorSize)
            {
                for (var i = 0; i < _denominatorSize; i++)
                {
                    _a[i] = _a[_denominatorSize + i] = a[i];
                }
            }
        }

        /// <summary>
        /// Reset filter
        /// </summary>
        public override void Reset()
        {
            _delayLineOffsetB = _numeratorSize - 1;
            _delayLineOffsetA = _denominatorSize - 1;

            for (var i = 0; i < _delayLineB.Length; _delayLineB[i++] = 0) { }
            for (var i = 0; i < _delayLineA.Length; _delayLineA[i++] = 0) { }
        }

        /// <summary>
        /// Divide all filter coefficients by _a[0] and normalize TF
        /// </summary>
        public void Normalize()
        {
            var a0 = _a[0];

            if (Math.Abs(a0 - 1) < 1e-10)
            {
                return;
            }

            if (Math.Abs(a0) < 1e-30)
            {
                throw new ArgumentException("The coefficient a[0] can not be zero!");
            }

            for (var i = 0; i < _a.Length; _a[i++] /= a0) { }
            for (var i = 0; i < _b.Length; _b[i++] /= a0) { }

            _tf?.Normalize();
        }
    }
}
