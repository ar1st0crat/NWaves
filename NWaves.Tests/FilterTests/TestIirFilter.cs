using NUnit.Framework;
using NWaves.Filters;
using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Tests.FilterTests
{
    [TestFixture]
    public class TestIirFilter
    {
        private readonly IirFilter _filter = 
            new IirFilter(new[] { 1, 0.4 }, new[] { 1, -0.6, 0.2 });

        private readonly DiscreteSignal _signal = 
            new DiscreteSignal(8000, new[] { 1.0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

        [Test]
        public void TestImpulseResponse()
        {
            AssertFilterOutput(_filter.ImpulseResponse());
        }

        [Test]
        public void TestFilterAppliedDirectly()
        {
            AssertFilterOutput(_filter.ApplyFilterDirectly(_signal));
        }

        [Test]
        public void TestFilterImplementedViaLinearBuffer()
        {
            AssertFilterOutput(_filter.ApplyFilterLinearBuffer(_signal));
        }

        [Test]
        public void TestFilterImplementedViaCircularBuffer()
        {
            AssertFilterOutput(_filter.ApplyFilterCircularBuffer(_signal));
        }

        [Test]
        public void TestFilterCombinations()
        {
            var pre = new PreEmphasisFilter();
            var de = new DeEmphasisFilter();

            var filter = pre * de;

            var samples = new[] { 1.0, 0.1, -0.4, 0.2 };
            var signal = new DiscreteSignal(1, samples);
            var filtered = filter.ApplyTo(signal);

            Assert.That(signal.Samples, Is.EqualTo(filtered.Samples).Within(1e-10));
        }

        private static void AssertFilterOutput(DiscreteSignal output)
        {
            Assert.That(output[0], Is.EqualTo(1.0).Within(1e-10));
            Assert.That(output[1], Is.EqualTo(1.0).Within(1e-10));
            Assert.That(output[2], Is.EqualTo(0.4).Within(1e-10));
            Assert.That(output[3], Is.EqualTo(0.04).Within(1e-10));
        }
    }
}
