using System.Linq;
using NUnit.Framework;
using NWaves.Utils;

namespace NWaves.Tests.Misc
{
    [TestFixture]
    public class TestMiscFunctions
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
