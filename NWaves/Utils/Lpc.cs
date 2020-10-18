using NWaves.Filters.Base;
using NWaves.Operations;
using NWaves.Signals;
using System;
using System.Linq;
using System.Numerics;

namespace NWaves.Utils
{
    /// <summary>
    /// Functions related to Linear Predictive Coding
    /// </summary>
    public static class Lpc
    {
        /// <summary>
        /// Levinson-Durbin algorithm for solving main LPC task
        /// </summary>
        /// <param name="input">Auto-correlation vector</param>
        /// <param name="a">LP coefficients</param>
        /// <param name="order">Order of LPC</param>
        /// <returns>Prediction error</returns>
        public static float LevinsonDurbin(float[] input, float[] a, int order, int offset = 0)
        {
            var err = input[offset];

            a[0] = 1.0f;

            for (var i = 1; i <= order; i++)
            {
                var lambda = 0.0f;
                for (var j = 0; j < i; j++)
                {
                    lambda -= a[j] * input[offset + i - j];
                }

                lambda /= err;

                for (var n = 0; n <= i / 2; n++)
                {
                    var tmp = a[i - n] + lambda * a[n];
                    a[n] = a[n] + lambda * a[i - n];
                    a[i - n] = tmp;
                }

                err *= (1.0f - lambda * lambda);
            }

            return err;
        }

        /// <summary>
        /// Convert LPC coefficients to cepstrum (LPCC)
        /// </summary>
        /// <param name="lpc"></param>
        /// <param name="gain"></param>
        /// <param name="lpcc"></param>
        public static void ToCepstrum(float[] lpc, float gain, float[] lpcc)
        {
            var n = lpcc.Length;
            var p = lpc.Length;     // must be lpcOrder + 1 (!)

            lpcc[0] = (float)Math.Log(gain);

            for (var m = 1; m < Math.Min(n, p); m++)
            {
                var acc = 0.0f;
                for (var k = 1; k < m; k++)
                {
                    acc += k * lpcc[k] * lpc[m - k];
                }
                lpcc[m] = -lpc[m] - acc / m;
            }

            for (var m = p; m < n; m++)
            {
                var acc = 0.0f;
                for (var k = 1; k < p; k++)
                {
                    acc += (m - k) * lpcc[m - k] * lpc[k];
                }
                lpcc[m] = -acc / m;
            }
        }

        /// <summary>
        /// Convert LPCC coefficients to LPC and gain
        /// 
        /// Formulae: https://www.mathworks.com/help/dsp/ref/lpctofromcepstralcoefficients.html
        /// 
        /// </summary>
        /// <param name="lpcc"></param>
        /// <param name="lpc"></param>
        /// <returns></returns>
        public static float FromCepstrum(float[] lpcc, float[] lpc)
        {
            var n = lpcc.Length;
            var p = lpc.Length;     // must be lpcOrder + 1 (!)

            lpc[0] = 1;

            for (var m = 1; m < p; m++)
            {
                var acc = 0.0f;
                for (var k = 1; k < m; k++)
                {
                    acc += k * lpcc[k] * lpc[m - k];
                }
                lpc[m] = -lpcc[m] - acc / m;
            }

            return (float)Math.Exp(lpcc[0]);
        }

        /// <summary>
        /// Method returns LPC order for a given sampling rate 
        /// according to the best practices.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <returns>LPC order</returns>
        public static int EstimateOrder(int samplingRate)
        {
            return 2 + samplingRate / 1000;
        }

        /// <summary>
        /// Convert LPC coefficients to Line Spectral Frequencies
        /// </summary>
        /// <param name="lpc"></param>
        /// <param name="lsf">The length must be equal to lpc length. Last element will be PI</param>
        public static void ToLsf(float[] lpc, float[] lsf)
        {
            var first = lpc[0];

            if (Math.Abs(first - 1) > 1e-10)
            {
                for (var i = 0; i < lpc.Length; lpc[i] /= first, i++) ;
            }

            var p = new float[lpc.Length + 1];
            var q = new float[lpc.Length + 1];

            p[0] = q[0] = 1;

            for (var i = 1; i < p.Length - 1; i++)
            {
                p[i] = lpc[i] - lpc[p.Length - i - 1];
                q[i] = lpc[i] + lpc[p.Length - i - 1];
            }

            p[p.Length - 1] = -1;
            q[q.Length - 1] = 1;

            var pRoots = MathUtils.PolynomialRoots(p.ToDoubles()).Select(r => r.Phase).ToArray();
            var qRoots = MathUtils.PolynomialRoots(q.ToDoubles()).Select(r => r.Phase).ToArray();

            Array.Sort(pRoots);
            Array.Sort(qRoots);

            var k = 0;
            for (var i = 0; i < qRoots.Length; i++)
            {
                if (qRoots[i] > 0) lsf[k++] = (float)qRoots[i];
            }
            for (var i = 0; i < pRoots.Length; i++)
            {
                if (pRoots[i] > 0) lsf[k++] = (float)pRoots[i];
            }

            Array.Sort(lsf);
        }

        /// <summary>
        /// Convert Line Spectral Frequencies to LPC coefficients
        /// </summary>
        /// <param name="lsf">The length must be equal to lpc length. Last element must be PI</param>
        /// <param name="lpc"></param>
        public static void FromLsf(float[] lsf, float[] lpc)
        {
            var n = lsf.Length - 1;
            var halfOrder = n / 2;

            var pPoles = new Complex[n];
            var qPoles = new Complex[n + 2 * (n % 2)];

            var k = 0;
            for (var i = 0; k < halfOrder; i += 2, k++)
            {
                qPoles[k] = new Complex(Math.Cos(lsf[i]), Math.Sin(lsf[i]));
                pPoles[k] = new Complex(Math.Cos(lsf[i + 1]), Math.Sin(lsf[i + 1]));
            }
            for (var i = 0; k < 2 * halfOrder; i += 2, k++)
            {
                qPoles[k] = new Complex(Math.Cos( lsf[i]), Math.Sin(-lsf[i]));
                pPoles[k] = new Complex(Math.Cos( lsf[i + 1]), Math.Sin(-lsf[i + 1]));
            }

            if (n % 2 == 1)
            {
                qPoles[n] = new Complex(Math.Cos(lsf[n - 1]), Math.Sin(lsf[n - 1]));
                qPoles[n + 1] = new Complex(Math.Cos( lsf[n - 1]), Math.Sin(-lsf[n - 1]));
            }

            var ps = new ComplexDiscreteSignal(1, TransferFunction.ZpToTf(pPoles));
            var qs = new ComplexDiscreteSignal(1, TransferFunction.ZpToTf(qPoles));

            if (n % 2 == 1)
            {
                ps = Operation.Convolve(ps, new ComplexDiscreteSignal(1, new[] { 1.0, 0, -1.0 }));
            }
            else
            {
                ps = Operation.Convolve(ps, new ComplexDiscreteSignal(1, new[] { 1.0, -1.0 }));
                qs = Operation.Convolve(qs, new ComplexDiscreteSignal(1, new[] { 1.0,  1.0 }));
            }

            for (var i = 0; i < lpc.Length; i++)
            {
                lpc[i] = (float)(0.5 * (ps.Real[i] + qs.Real[i]));
            }
        }
    }
}
