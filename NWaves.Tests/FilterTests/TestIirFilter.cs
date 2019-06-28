using NUnit.Framework;
using NWaves.Filters;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Tests.FilterTests
{
    [TestFixture]
    public class TestIirFilter
    {
        private readonly IirFilter _filter = 
            new IirFilter(new[] { 1, 0.4 }, new[] { 1, -0.6, 0.2 });

        private readonly DiscreteSignal _signal = 
            new DiscreteSignal(8000, new[] { 1.0f, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

        [Test]
        public void TestFilterAppliedDirectly()
        {
            AssertFilterOutput(_filter.ApplyFilterDirectly(_signal));
        }

        [Test]
        public void TestImpulseResponse()
        {
            AssertFilterOutput(new DiscreteSignal(1, _filter.Tf.ImpulseResponse().ToFloats()));
        }

        [Test]
        public void TestFilterCombinations()
        {
            var pre = new PreEmphasisFilter();
            var de = new DeEmphasisFilter();

            var filter = pre * de;

            var samples = new[] { 1.0f, 0.1f, -0.4f, 0.2f };
            var signal = new DiscreteSignal(1, samples);
            var filtered = filter.ApplyTo(signal);

            Assert.That(filtered.Samples, Is.EqualTo(signal.Samples).Within(1e-7));
        }

        private static void AssertFilterOutput(DiscreteSignal output)
        {
            Assert.Multiple(() =>
            {
                Assert.That(output[0], Is.EqualTo(1.0).Within(1e-7));
                Assert.That(output[1], Is.EqualTo(1.0).Within(1e-7));
                Assert.That(output[2], Is.EqualTo(0.4).Within(1e-7));
                Assert.That(output[3], Is.EqualTo(0.04).Within(1e-7));
            });
        }
    }
}
