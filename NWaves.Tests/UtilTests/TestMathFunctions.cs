using System.Linq;
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

            var interp = MathUtils.InterpolateLinear(x, y, arg);

            Assert.That(interp, Is.EqualTo(new[] { 5, 12.142857f, 11.142857f, 5.428571f, 2.571429f }).Within(1e-4));
        }

        [Test]
        public void TestLinearInterpolationSmallStep()
        {
            var x = new[] { 0, 0.4f, 0.8f, 1.2f, 1.6f, 2.0f, 2.4f, 2.8f, 3.2f };
            var y = new[] { 5, 15, 6, 2, 8, 4, 10, 6, 2.0f };
            var arg = new[] { 0, 1, 2, 3.0f };

            var interp = MathUtils.InterpolateLinear(x, y, arg);

            Assert.That(interp, Is.EqualTo(new[] { 5.0f, 4, 4, 4 }).Within(1e-4));
        }

        [Test]
        public void TestRealPolynomialRoots()
        {
            double[] re = { -6, -5, 2, 1 };
            double[] im = { 0, 0, 0, 0 };

            var roots = MathUtils.PolynomialRoots(re, im);

            double[] expected = {2, -3, -1};
            Assert.That(roots.Item1, Is.EquivalentTo(expected));
        }

        [Test]
        public void TestComplexPolynomialRoots()
        {
            double[] re = { 2.5, 1, 1 };
            double[] im = { 0, 0, 0 };

            var roots = MathUtils.PolynomialRoots(re, im);

            double[] expectedReal = { -0.5, -0.5 };
            double[] expectedImag = { -1.5, 1.5 };
            Assert.That(roots.Item1.OrderBy(r => r), Is.EqualTo(expectedReal).Within(1e-10));
            Assert.That(roots.Item2.OrderBy(r => r), Is.EqualTo(expectedImag).Within(1e-10));
        }
    }
}
