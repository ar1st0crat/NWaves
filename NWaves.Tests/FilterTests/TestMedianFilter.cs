using NUnit.Framework;
using NWaves.Filters;
using NWaves.Signals;

namespace NWaves.Tests.FilterTests
{
    [TestFixture]
    public class TestMedianFilter
    {
        private readonly MedianFilter _filter = new MedianFilter(5);

        [Test]
        public void TestMedianFiltering()
        {
            var input = new[] { 1.0f, 12, 2, 3, 2, 1, 14, 16, 7, 3, 4, 6, 12, 4, 1, 6, 14, 2, 5 };
            var expected = new[] { 2.0f, 2, 2, 2, 2, 2, 2, 3, 7, 7, 7, 6, 6, 4, 4, 6, 6, 4, 5 };

            var filtered = _filter.ApplyTo(new DiscreteSignal(1, input));

            Assert.That(filtered.Samples, Is.EqualTo(expected).Within(1e-10));
        }
    }
}
