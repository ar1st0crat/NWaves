using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using NWaves.Operations;
using NWaves.Operations.Convolution;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Represents the transfer function of an LTI filter.
    /// </summary>
    public class TransferFunction
    {
        /// <summary>
        /// Gets numerator of transfer function.
        /// </summary>
        public double[] Numerator { get; protected set; }

        /// <summary>
        /// Gets denominator of transfer function.
        /// </summary>
        public double[] Denominator { get; protected set; }

        /// <summary>
        /// Gets or sets max number of iterations for calculating zeros/poles (roots of polynomials). By default, 25000.
        /// </summary>
        public int CalculateZpIterations { get; set; } = MathUtils.PolyRootsIterations;

        /// <summary>
        /// Zeros.
        /// </summary>
        protected Complex[] _zeros;

        /// <summary>
        /// Poles.
        /// </summary>
        protected Complex[] _poles;

        /// <summary>
        /// Gets zeros ('z' in 'zpk' notation).
        /// </summary>
        public Complex[] Zeros => _zeros ?? TfToZp(Numerator, CalculateZpIterations);

        /// <summary>
        /// Gets poles ('p' in 'zpk' notation).
        /// </summary>
        public Complex[] Poles => _poles ?? TfToZp(Denominator, CalculateZpIterations);

        /// <summary>
        /// Gets gain ('k' in 'zpk' notation).
        /// </summary>
        public double Gain => Numerator[0];

        /// <summary>
        /// Constructs <see cref="TransferFunction"/> from <paramref name="numerator"/> and <paramref name="denominator"/>.
        /// </summary>
        /// <param name="numerator">Numerator of transfer function</param>
        /// <param name="denominator">Denominator of transfer function</param>
        public TransferFunction(double[] numerator, double[] denominator = null)
        {
            Numerator = numerator;
            Denominator = denominator ?? new [] { 1.0 };
        }

        /// <summary>
        /// Constructs <see cref="TransferFunction"/> from <paramref name="zeros"/>, <paramref name="poles"/> and <paramref name="gain"/>.
        /// </summary>
        /// <param name="zeros">Zeros</param>
        /// <param name="poles">Poles</param>
        /// <param name="gain">Gain</param>
        public TransferFunction(Complex[] zeros, Complex[] poles, double gain = 1)
        {
            _zeros = zeros;
            _poles = poles;

            Denominator = poles != null ? ZpToTf(poles) : new[] { 1.0 };
            Numerator = zeros != null ? ZpToTf(zeros) : new[] { 1.0 };

            for (var i = 0; i < Numerator.Length; i++)
            {
                Numerator[i] *= gain;
            }
        }

        /// <summary>
        /// Constructs <see cref="TransferFunction"/> from <paramref name="zeros"/>, <paramref name="poles"/> and <paramref name="gain"/>.
        /// </summary>
        /// <param name="zeros">Zeros</param>
        /// <param name="poles">Poles</param>
        /// <param name="gain">Gain</param>
        public TransferFunction(ComplexDiscreteSignal zeros, ComplexDiscreteSignal poles, double gain = 1)
            : this(zeros.ToComplexNumbers().ToArray(), poles.ToComplexNumbers().ToArray(), gain)
        {
        }

        /// <summary>
        /// Constructs <see cref="TransferFunction"/> from <paramref name="stateSpace"/> representation.
        /// </summary>
        /// <param name="stateSpace">State space representation</param>
        public TransferFunction(StateSpace stateSpace)
        {
            var a = stateSpace.A;

            Denominator = new double[a.Length + 1];
            Denominator[0] = 1;
            for (var i = 1; i < Denominator.Length; i++)
            {
                Denominator[i] = -a[0][i - 1];
            }

            var c = stateSpace.C;
            var d = stateSpace.D;

            var num = new double[a.Length + 1];

            for (var i = 0; i < a.Length; i++)
            {
                num[i + 1] = -(a[0][i] - c[i]) + (d[0] - 1) * Denominator[i + 1];
            }

            const double ZeroTolerance = 1e-8;

            var index = 0;
            for (var i = 1; i < num.Length; i++)
            {
                if (Math.Abs(num[i]) > ZeroTolerance)
                {
                    index = i;
                    break;
                }
            }

            if (Math.Abs(d[0]) > ZeroTolerance)
            {
                index--;
            }

            Numerator = num.FastCopyFragment(num.Length - index, index);

            if (Math.Abs(d[0]) > ZeroTolerance)
            {
                Numerator[0] = d[0];
            }
        }

        /// <summary>
        /// Gets state-space representation.
        /// </summary>
        public StateSpace StateSpace
        {
            get
            {
                var M = Numerator.Length;
                var K = Denominator.Length;

                if (M > K)
                {
                    throw new ArgumentException("Numerator size must not exceed denominator size");
                }

                var a0 = Denominator[0];    // normalize: all further results will be divided by a0

                if (K == 1)
                {
                    return new StateSpace
                    {
                        A = new Matrix(1).As2dArray(),
                        B = new double[M],
                        C = new double[M],
                        D = new double[1] { Numerator[0] / a0 }
                    };
                }

                var num = Numerator;

                if (M < K)
                {
                    num = new double[K];
                    Numerator.FastCopyTo(num, M, 0, K - M);
                }

                var a = new Matrix(K - 1);
                for (var i = 0; i < K - 1; i++)
                {
                    a[0][i] = -Denominator[i + 1] / a0;
                }
                for (var i = 1; i < K - 1; i++)
                {
                    a[i][i - 1] = 1;
                }

                var b = new double[K - 1];
                b[0] = 1;

                var c = new double[K - 1];
                for (var i = 0; i < K - 1; i++)
                {
                    c[i] = (num[i + 1] - num[0] * Denominator[i + 1] / a0) / a0;
                }

                var d = new double[1] { num[0] / a0 };

                return new StateSpace
                {
                    A = a.As2dArray(),
                    B = b,
                    C = c,
                    D = d
                };
            }
        }

        /// <summary>
        /// Gets initial state for <see cref="ZiFilter"/> that corresponds to the steady state of the step response.
        /// </summary>
        public double[] Zi
        {
            get
            {
                var size = Math.Max(Numerator.Length, Denominator.Length);

                var a = Denominator.PadZeros(size);
                var b = Numerator.PadZeros(size);

                var a0 = a[0];

                for (var i = 0; i < a.Length; a[i++] /= a0) ;
                for (var i = 0; i < b.Length; b[i++] /= a0) ;

                var B = new double[size - 1];

                for (var i = 1; i < size; i++)
                {
                    B[i - 1] = b[i] - a[i] * b[0];
                }

                Matrix m = Matrix.Eye(size - 1) - Matrix.Companion(a).T;

                var sum = 0.0;

                for (var i = 0; i < size - 1; i++)
                {
                    sum += m[i][0];
                }

                var zi = new double[size];

                zi[0] = B.Sum() / sum;

                var asum = 1.0;
                var csum = 0.0;
                for (var i = 1; i < size - 1; i++)
                {
                    asum += a[i];
                    csum += b[i] - a[i] * b[0];
                    zi[i] = asum * zi[0] - csum;
                }

                return zi;
            }
        }

        /// <summary>
        /// Evaluates impulse response of given <paramref name="length"/>. 
        /// In case of FIR filters method returns full copy of numerator.
        /// </summary>
        /// <param name="length">Length of the impulse response</param>
        public double[] ImpulseResponse(int length = 512)
        {
            if (Denominator.Length == 1)
            {
                return Numerator.FastCopy();
            }

            var b = Numerator;
            var a = Denominator;

            var response = new double[length];

            for (var n = 0; n < response.Length; n++)
            {
                if (n < b.Length) response[n] = b[n];

                for (var m = 1; m < a.Length; m++)
                {
                    if (n >= m) response[n] -= a[m] * response[n - m];
                }
            }

            return response;
        }

        /// <summary>
        /// Evaluates frequency response of given <paramref name="length"/>.
        /// </summary>
        /// <param name="length">Length of the frequency response</param>
        public ComplexDiscreteSignal FrequencyResponse(int length = 512)
        {
            var ir = ImpulseResponse(length);

            var real = ir.Length == length ? ir :
                       ir.Length  < length ? ir.PadZeros(length) :
                                             ir.FastCopyFragment(length);
            var imag = new double[length];

            var fft = new Fft64(length);
            fft.Direct(real, imag);

            return new ComplexDiscreteSignal(1, real.Take(length / 2 + 1),
                                                imag.Take(length / 2 + 1));
        }

        /// <summary>
        /// Evaluates group delay in array of given <paramref name="length"/>.
        /// </summary>
        /// <param name="length">Length of group delay array</param>
        public double[] GroupDelay(int length = 512)
        {
            var cc = new ComplexConvolver()
                            .CrossCorrelate(new ComplexDiscreteSignal(1, Numerator),
                                            new ComplexDiscreteSignal(1, Denominator)).Real;

            var cr = Enumerable.Range(0, cc.Length)
                               .Zip(cc, (r, c) => r * c)
                               .Reverse()
                               .ToArray();

            cc = cc.Reverse().ToArray();    // reverse cc and cr (above) for EvaluatePolynomial()

            var step = Math.PI / length;
            var omega = 0.0;
            
            var dn = Denominator.Length - 1;

            var gd = new double[length];

            for (var i = 0; i < gd.Length; i++)
            {
                var z = Complex.FromPolarCoordinates(1, -omega);
                var num = MathUtils.EvaluatePolynomial(cr, z);
                var den = MathUtils.EvaluatePolynomial(cc, z);

                gd[i] = Complex.Abs(den) < 1e-30 ? 0 : (num / den).Real - dn;

                omega += step;
            }

            return gd;
        }

        /// <summary>
        /// Evaluates phase delay in array of given <paramref name="length"/>.
        /// </summary>
        /// <param name="length">Length of phase delay array</param>
        public double[] PhaseDelay(int length = 512)
        {
            var gd = GroupDelay(length);

            var pd = new double[gd.Length];
            var acc = 0.0;
            for (var i = 0; i < pd.Length; i++)     // integrate group delay
            {
                acc += gd[i];
                pd[i] = acc / (i + 1);
            }

            return pd;
        }

        /// <summary>
        /// Normalizes frequency response at given frequency 
        /// (normalizes numerator to map frequency response onto [0, 1])
        /// </summary>
        /// <param name="freq">Frequency</param>
        public void NormalizeAt(double freq)
        {
            var w = Complex.FromPolarCoordinates(1, freq);

            var gain = Complex.Abs(MathUtils.EvaluatePolynomial(Denominator, w) /
                                   MathUtils.EvaluatePolynomial(Numerator, w));

            for (var i = 0; i < Numerator.Length; i++)
            {
                Numerator[i] *= gain;
            }
        }

        /// <summary>
        /// Normalizes numerator and denominator (divides them by the first coefficient of denominator).
        /// </summary>
        public void Normalize()
        {
            var a0 = Denominator[0];

            if (Math.Abs(a0) < 1e-10)
            {
                throw new ArgumentException("The first denominator coefficient can not be zero!");
            }

            for (var i = 0; i < Denominator.Length; i++)
            {
                Denominator[i] /= a0;
            }

            for (var i = 0; i < Numerator.Length; i++)
            {
                Numerator[i] /= a0;
            }
        }

        /// <summary>
        /// Converts zeros (or poles) to numerator (or denominator) of transfer function.
        /// </summary>
        /// <param name="zp">Zeros (or poles)</param>
        public static double[] ZpToTf(Complex[] zp)
        {
            var poly = new Complex[] { 1, -zp[0] };

            for (var k = 1; k < zp.Length; k++)
            {
                var poly1 = new Complex[] { 1, -zp[k] };
                poly = MathUtils.MultiplyPolynomials(poly, poly1);
            }

            return poly.Select(p => p.Real).ToArray();
        }

        /// <summary>
        /// Converts zeros (or poles) to numerator (or denominator) of transfer function.
        /// </summary>
        /// <param name="zp">Zeros (or poles)</param>
        public static double[] ZpToTf(ComplexDiscreteSignal zp)
        {
            return ZpToTf(zp.ToComplexNumbers().ToArray());
        }

        /// <summary>
        /// Converts zeros (or poles) to numerator (or denominator) of transfer function. 
        /// Zeros (poles) are given in the form double arrays of real and imaginary parts.
        /// </summary>
        /// <param name="re">Real parts of complex zeros (poles)</param>
        /// <param name="im">Imaginary parts of complex zeros (poles)</param>
        public static double[] ZpToTf(double[] re, double[] im = null)
        {
            if (im is null)
            {
                im = new double[re.Length];
            }

            return ZpToTf(re.Zip(im, (r, i) => new Complex(r, i)).ToArray());
        }

        /// <summary>
        /// Converts numerator (or denominator) of transfer function to zeros (or poles).
        /// </summary>
        /// <param name="numeratorOrDenominator">Numerator or denominator (polynomial)</param>
        /// <param name="maxIterations">Max number of iterations for calculating zeros/poles (roots of polynomials). By default, 25000.</param>
        public static Complex[] TfToZp(double[] numeratorOrDenominator, int maxIterations = MathUtils.PolyRootsIterations)
        {
            if (numeratorOrDenominator.Length <= 1)
            {
                return null;
            }

            return MathUtils.PolynomialRoots(numeratorOrDenominator, maxIterations);
        }

        /// <summary>
        /// Creates transfer function from sequential connection of <paramref name="tf1"/> and <paramref name="tf2"/>.
        /// </summary>
        /// <param name="tf1">First transfer function</param>
        /// <param name="tf2">Second transfer function</param>
        public static TransferFunction operator *(TransferFunction tf1, TransferFunction tf2)
        {
            var num = Operation.Convolve(tf1.Numerator, tf2.Numerator);
            var den = Operation.Convolve(tf1.Denominator, tf2.Denominator);

            return new TransferFunction(num, den);
        }

        /// <summary>
        /// Creates transfer function from parallel connection of <paramref name="tf1"/> and <paramref name="tf2"/>.
        /// </summary>
        /// <param name="tf1">First transfer function</param>
        /// <param name="tf2">Second transfer function</param>
        public static TransferFunction operator +(TransferFunction tf1, TransferFunction tf2)
        {
            var num1 = Operation.Convolve(tf1.Numerator, tf2.Denominator);
            var num2 = Operation.Convolve(tf2.Numerator, tf1.Denominator);

            var num = num1;
            var add = num2;

            if (num1.Length < num2.Length)
            {
                num = num2;
                add = num1;
            }

            for (var i = 0; i < add.Length; i++)
            {
                num[i] += add[i];
            }

            var den = Operation.Convolve(tf1.Denominator, tf2.Denominator);

            return new TransferFunction(num, den);
        }

        /// <summary>
        /// Loads numerator and denominator of transfer function from csv stream.
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="delimiter">Delimiter</param>
        public static TransferFunction FromCsv(Stream stream, char delimiter = ',')
        {
            using (var reader = new StreamReader(stream))
            {
                var content = reader.ReadLine();
                var numerator = content.Split(delimiter)
                                       .Select(s => double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture))
                                       .ToArray();

                content = reader.ReadLine();
                var denominator = content.Split(delimiter)
                                         .Select(s => double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture))
                                         .ToArray();

                return new TransferFunction(numerator, denominator);
            }
        }

        /// <summary>
        /// Serializes numerator and denominator of transfer function to csv stream.
        /// </summary>
        /// <param name="stream">Output stream</param>
        /// <param name="delimiter">Delimiter</param>
        public void ToCsv(Stream stream, char delimiter = ',')
        {
            using (var writer = new StreamWriter(stream))
            {
                var content = string.Join(delimiter.ToString(), Numerator.Select(k => k.ToString(CultureInfo.InvariantCulture)));
                writer.WriteLine(content);

                content = string.Join(delimiter.ToString(), Denominator.Select(k => k.ToString(CultureInfo.InvariantCulture)));
                writer.WriteLine(content);
            }
        }
    }
}
