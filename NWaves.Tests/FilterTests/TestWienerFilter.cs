using NUnit.Framework;
using NWaves.Filters;
using NWaves.Signals;

namespace NWaves.Tests.FilterTests
{
    [TestFixture]
    public class TestWienerFilter
    {
        [Test]
        public void TestWienerFilteringBigNoise()
        {
            var filter = new WienerFilter(3, 1000);

            var input = new[] { 5, 4, 6, 2, 1f };
            var expected = new[] { 3, 5, 4, 3, 1f };

            var filtered = filter.ApplyTo(new DiscreteSignal(1, input));

            Assert.That(filtered.Samples, Is.EqualTo(expected).Within(1e-10));
        }

        [Test]
        public void TestWienerFilteringNoNoise()
        {
            var filter = new WienerFilter(3, 0.1);

            var input = new[] { 5, 4, 6, 2, 1f };
            var expected = new[] { 4.95714286f, 4.15f, 5.925f, 2.02142857f, 1 };

            var filtered = filter.ApplyTo(new DiscreteSignal(1, input));

            Assert.That(filtered.Samples, Is.EqualTo(expected).Within(1e-5));
        }
    }
}
