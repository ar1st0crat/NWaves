using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.Transforms.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Linear Predictive Coding coefficients extractor
    /// </summary>
    public class LpcExtractor : IFeatureExtractor
    {
        /// <summary>
        /// Number of features coincides with the order of LPC
        /// </summary>
        public int FeatureCount => _order;

        /// <summary>
        /// Descriptions ("error", "lp coefficient1", "lp coefficient2", etc.)
        /// </summary>
        public IEnumerable<string> FeatureDescriptions
        {
            get
            {
                return new [] { "error" }.Concat(
                    Enumerable.Range(1, FeatureCount).Select(i => "lp coefficient" + i));
            }
        }
            
        /// <summary>
        /// Order of an LPC-filter
        /// </summary>
        private readonly int _order;

        /// <summary>
        /// Size of analysis window
        /// </summary>
        private readonly int _windowSize;

        /// <summary>
        /// Pre-emphasis filter (if needed)
        /// </summary>
        private readonly PreEmphasisFilter _preemphasisFilter;

        /// <summary>
        /// Array used in andvanced Levinson-Durbin recursive algorithm
        /// </summary>
        private readonly double[] _tmp;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="windowSize"></param>
        /// <param name="order"></param>
        /// <param name="samplingRate"></param>
        /// <param name="preEmphasis"></param>
        public LpcExtractor(int windowSize, int order = 0, int samplingRate = 16000, double preEmphasis = 0.0)
        {
            _order = (order > 0) ? order : 2 + samplingRate / 1000;
            _windowSize = windowSize;

            if (preEmphasis > 0.0)
            {
                _preemphasisFilter = new PreEmphasisFilter(preEmphasis);
            }

            // for advanced Levinson-Durbin
            _tmp = new double[_order + 1];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="order"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public double LevinsonDurbin(double[] input, int order, double[] a)
        {
            var err = input[0];

            a[0] = 1.0;

            for (var i = 1; i <= order; i++)
            {
                var lambda = 0.0;
                for (var j = 0; j < i; j++)
                {
                    lambda -= a[j] * input[i - j];
                }

                lambda /= err;

                for (var n = 0; n <= i / 2; n++)
                {
                    var tmp = a[i - n] + lambda * a[n];
                    a[n] = a[n] + lambda * a[i - n];
                    a[i - n] = tmp;
                }

                err *= (1.0 - lambda * lambda);
            }

            return err;
        }

        /// <summary>
        /// Advanced version of Levinson-Durbin recursion
        /// </summary>
        /// <param name="input"></param>
        /// <param name="order"></param>
        /// <param name="a"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public double LevinsonDurbinAdvanced(double[] input, int order, double[] a, double[] k)
        {
            for (var i = 0; i <= order; i++)
            {
                _tmp[i] = 0.0;
            }

            var err = input[0];

            a[0] = 1.0;

            for (var i = 1; i <= order; ++i)
            {
                var acc = input[i];

                for (var j = 1; j <= i - 1; ++j)
                {
                    acc += a[j] * input[i - j];
                }

                k[i - 1] = -acc / err;
                a[i] = k[i - 1];

                for (var j = 0; j < order; ++j)
                {
                    _tmp[j] = a[j];
                }

                for (var j = 1; j < i; ++j)
                {
                    a[j] += k[i - 1] * _tmp[i - j];
                }

                err *= (1 - k[i - 1] * k[i - 1]);
            }

            return err;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal)
        {
            var featureVectors = new List<FeatureVector>();

            // 0) pre-emphasis (if needed)

            var filtered = (_preemphasisFilter != null) ? _preemphasisFilter.ApplyTo(signal) : signal;

            var i = 0;
            while (i + _windowSize < filtered.Samples.Length)
            {
                var x = filtered[i, i + _windowSize];

                // 1) autocorr

                var cc = Operation.CrossCorrelate(x, x).Last(_windowSize);

                // 2) levinson-durbin

                var a = new double[_order + 1];
                var err = LevinsonDurbin(cc.Samples, _order, a);
                a[0] = err;

                // add LPC vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = a,
                    TimePosition = (double)i / signal.SamplingRate
                });

                i += _windowSize;
            }

            return featureVectors;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        public IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal, int startPos, int endPos)
        {
            return ComputeFrom(signal[startPos, endPos]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samples"></param>
        /// <returns></returns>
        public IEnumerable<FeatureVector> ComputeFrom(IEnumerable<double> samples)
        {
            return ComputeFrom(new DiscreteSignal(1, samples));
        }
    }
}
