using System.Linq;
using System.Numerics;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Transfer function
    /// </summary>
    public static class TransferFunction
    {
        /// <summary>
        /// Method for converting zeros(poles) to TF numerator(denominator)
        /// </summary>
        /// <param name="zp"></param>
        /// <returns></returns>
        public static double[] ZpToTf(Complex[] zp)
        {
            var tf = new ComplexDiscreteSignal(1, new[] { 1.0, -zp[0].Real },
                                                  new[] { 0.0, -zp[0].Imaginary });

            for (var k = 1; k < zp.Length; k++)
            {
                tf = Operation.Convolve(tf, new ComplexDiscreteSignal(1, 
                                                    new[] { 1.0, -zp[k].Real },
                                                    new[] { 0.0, -zp[k].Imaginary }));
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
            if (im == null)
            {
                im = new double[re.Length];
            }

            return ZpToTf(re.Zip(im, (r, i) => new Complex(r, i)).ToArray());
        }

        /// <summary>
        /// Method for converting TF numerator(denominator) to zeros(poles)
        /// </summary>
        /// <param name="tf"></param>
        /// <returns></returns>
        public static Complex[] TfToZp(double[] tf)
        {
            return tf.Length <= 1 ? null : MathUtils.PolynomialRoots(tf);
        }
    }
}
