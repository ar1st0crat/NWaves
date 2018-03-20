using System.Linq;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Class providing methods related to the transfer function of a filter
    /// </summary>
    public static class TransferFunction
    {
        /// <summary>
        /// Method for converting zeros(poles) to TF numerator(denominator)
        /// </summary>
        /// <param name="zp"></param>
        /// <returns></returns>
        public static double[] ZpToTf(ComplexDiscreteSignal zp)
        {
            var tf = new ComplexDiscreteSignal(1, new[] { 1.0, -zp.Real[0] },
                                                  new[] { 0.0, -zp.Imag[0] });

            for (var k = 1; k < zp.Length; k++)
            {
                tf = Operation.Convolve(tf, new ComplexDiscreteSignal(1, 
                                                  new[] { 1.0, -zp.Real[k] },
                                                  new[] { 0.0, -zp.Imag[k] }));
            }

            return tf.Real;
        }

        /// <summary>
        /// Method for converting zeros(poles) to TF numerator(denominator).
        /// Zeros and poles are given as double arrays of real and imaginary parts of zeros(poles).
        /// </summary>
        /// <param name="re"></param>
        /// <param name="im"></param>
        /// <returns></returns>
        public static double[] ZpToTf(double[] re, double[] im = null)
        {
            return ZpToTf(new ComplexDiscreteSignal(1, re, im));
        }

        /// <summary>
        /// Method for converting TF numerator(denominator) to zeros(poles)
        /// </summary>
        /// <param name="tf"></param>
        /// <returns></returns>
        public static ComplexDiscreteSignal TfToZp(double[] tf)
        {
            if (tf.Length <= 1)
            {
                return null;
            }

            var roots = MathUtils.PolynomialRoots(tf);

            return new ComplexDiscreteSignal(1, roots.Select(r => r.Real),
                                                roots.Select(r => r.Imaginary));
        }
    }
}
