using System.Linq;
using System.Numerics;
using NUnit.Framework;
using NWaves.Utils;

namespace NWaves.Tests.UtilTests
{
    [TestFixture]
    public class TestMathFunctions
    {
        [Test]
        public void TestNextPowerOfTwo()
        {
            Assert.That(MathUtils.NextPowerOfTwo(64), Is.EqualTo(64));
            Assert.That(MathUtils.NextPowerOfTwo(63), Is.EqualTo(64));
            Assert.That(MathUtils.NextPowerOfTwo(33), Is.EqualTo(64));
            Assert.That(MathUtils.NextPowerOfTwo(65000), Is.EqualTo(65536));
        }

        [Test]
        public void TestLinearInterpolationBigStep()
        {
            var x = new[] { 0, 1.4f, 2.8f, 4.2f, 5.6f, 7.0f, 8.4f };
            var y = new[] { 5, 15, 6, 2, 8, 4, 10.0f };
            var arg = new[] { 0, 1, 2, 3, 4.0f };
            var interp = new float[arg.Length];

            MathUtils.InterpolateLinear(x, y, arg, interp);

            Assert.That(interp, Is.EqualTo(new[] { 5, 12.142857f, 11.142857f, 5.428571f, 2.571429f }).Within(1e-4));
        }

        [Test]
        public void TestLinearInterpolationSmallStep()
        {
            var x = new[] { 0, 0.4f, 0.8f, 1.2f, 1.6f, 2.0f, 2.4f, 2.8f, 3.2f };
            var y = new[] { 5, 15, 6, 2, 8, 4, 10, 6, 2.0f };
            var arg = new[] { 0, 1, 2, 3.0f };
            var interp = new float[arg.Length];

            MathUtils.InterpolateLinear(x, y, arg, interp);

            Assert.That(interp, Is.EqualTo(new[] { 5.0f, 4, 4, 4 }).Within(1e-4));
        }

        [Test]
        public void TestRealPolynomialRoots()
        {
            double[] re = { 1, 2, -5, -6 };
            
            var roots = MathUtils.PolynomialRoots(re);

            double[] expected = {-3, -1, 2};
            Assert.That(roots.Select(r => r.Real).OrderBy(r => r), Is.EqualTo(expected).Within(1e-7));
        }

        [Test]
        public void TestComplexPolynomialRoots()
        {
            double[] re = { 1, 1, 2.5 };
            
            var roots = MathUtils.PolynomialRoots(re);

            double[] expectedReal = { -0.5, -0.5 };
            double[] expectedImag = { -1.5, 1.5 };

            Assert.Multiple(() =>
            {
                Assert.That(roots.Select(r => r.Real).OrderBy(r => r), Is.EqualTo(expectedReal).Within(1e-7));
                Assert.That(roots.Select(r => r.Imaginary).OrderBy(r => r), Is.EqualTo(expectedImag).Within(1e-7));
            });
        }

        [Test]
        public void TestPolynomialDivision()
        {
            Complex[] num = { 2, 4, 6, 1, 2, 3 };
            Complex[] den = { 1, 2, 3 };

            var div = MathUtils.DividePolynomial(num, den);

            Assert.Multiple(() =>
            {
                Assert.That(div[0].Select(d => d.Real), Is.EqualTo(new[] {2.0, 0, 0, 1}).Within(1e-10));
                Assert.That(div[0].Select(d => d.Imaginary), Is.EqualTo(new[] { 0.0, 0, 0, 0 }).Within(1e-10));
                Assert.That(div[1].Select(d => d.Real), Is.All.EqualTo(0.0).Within(1e-10));
            });
        }

        [Test]
        public void TestLpcToLsf()
        {
            //float[] lpc = { 1, 0.6149f, 0.9899f, 0, 0.0031f, -0.008f, 0.0154f };
            //float[] lsf = new float[lpc.Length];
            //Lpc.ToLsf(lpc, lsf);
            //Assert.That(lsf, Is.EqualTo(new [] { 0.62694603f, 1.25538484f, 1.82578472f, 1.87689099f, 1.95275509f, 2.51259995f, 3.1415927f }).Within(1e-5));

            float[] lpc = { 1, 0.6149f, 0.2899f, 0.0031f, -0.0082f, -0.123f };
            float[] lsf = new float[lpc.Length];

            Lpc.ToLsf(lpc, lsf);

            Assert.That(lsf, Is.EqualTo(new[] { 0.6471242f, 1.29403331f, 1.74836394f, 2.26815244f, 2.62021719f, 3.1415927f}).Within(1e-5));
        }

        [Test]
        public void TestLsfToLpc()
        {
            //float[] lsf = { 0.62694603f, 1.25538484f, 1.82578472f, 1.87689099f, 1.95275509f, 2.51259995f, 3.1415927f };
            //float[] lpc = new float[lsf.Length];
            //Lpc.FromLsf(lsf, lpc);
            //Assert.That(lpc, Is.EqualTo(new[] { 1, 0.6149f, 0.9899f, 0, 0.0031f, -0.008f, 0.0154f }).Within(1e-5));

            float[] lsf = { 0.783008181f, 1.294033314f, 1.56781325f, 2.26815244f, 2.849793301f, 3.1415927f };
            float[] lpc = new float[lsf.Length];

            Lpc.FromLsf(lsf, lpc);

            Assert.That(lpc, Is.EqualTo(new[] { 1, 0.6149f, 0.2899f, 0.5f, -0.0082f, -0.123f }).Within(1e-5));
        }
    }
}