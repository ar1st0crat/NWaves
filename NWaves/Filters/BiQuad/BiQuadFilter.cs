using NWaves.Filters.Base;
using System;

namespace NWaves.Filters.BiQuad
{
    /// <summary>
    /// Represents BiQuad IIR filter.
    /// </summary>
    public class BiQuadFilter : IirFilter
    {
        // Delay line

        private float _in1;
        private float _in2;
        private float _out1;
        private float _out2;

        /// <summary>
        /// Constructs <see cref="BiQuadFilter"/>.
        /// </summary>
        protected BiQuadFilter() : base(new[] { 1.0, 0, 0 }, new[] { 1.0, 0, 0 })
        {
        }

        /// <summary>
        /// Constructs <see cref="BiQuadFilter"/> from filter coefficients 
        /// (numerator {B0, B1, B2} and denominator {A0, A1, A2}).
        /// </summary>
        /// <param name="b0">B0</param>
        /// <param name="b1">B1</param>
        /// <param name="b2">B2</param>
        /// <param name="a0">A0</param>
        /// <param name="a1">A1</param>
        /// <param name="a2">A2</param>
        public BiQuadFilter(double b0, double b1, double b2,
                            double a0, double a1, double a2) : 
            base(new[] { b0, b1, b2 }, new[] { a0, a1, a2 })
        {
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var output = _b[0] * sample + _b[1] * _in1 + _b[2] * _in2 - _a[1] * _out1 - _a[2] * _out2;

            _in2 = _in1;
            _in1 = sample;
            _out2 = _out1;
            _out1 = output;

            return output;
        }

        /// <summary>
        /// Resets filter.
        /// </summary>
        public override void Reset()
        {
            _in1 = _in2 = _out1 = _out2 = 0;
        }

        /// <summary>
        /// Changes filter coefficients online (preserving the state of the filter).
        /// </summary>
        /// <param name="b0">B0</param>
        /// <param name="b1">B1</param>
        /// <param name="b2">B2</param>
        /// <param name="a0">A0</param>
        /// <param name="a1">A1</param>
        /// <param name="a2">A2</param>
        public void Change(float b0, float b1, float b2, float a0, float a1, float a2)
        {
            if (Math.Abs(a0) < 1e-30f)
            {
                throw new ArgumentException("The coefficient a0 can not be zero!");
            }

            _b[0] = b0 / a0;
            _b[1] = b1 / a0;
            _b[2] = b2 / a0;
            _a[1] = a1 / a0;
            _a[2] = a2 / a0;
        }
    }
}
