using NUnit.Framework;
using NWaves.Operations;
using NWaves.Signals;

namespace NWaves.Tests.OperationTests
{
    [TestFixture]
    public class TestResampling
    {
        [Test]
        public void TestSameSamplingRate()
        {
            var signal = new DiscreteSignal(16000, new [] {1.0, -2, 3, 1, 4, -2, 1, -5, 3});

            var resampled = Operation.Resample(signal, 16000);

            Assert.Multiple(() =>
            {
                Assert.That(resampled.Samples, Is.EqualTo(resampled.Samples).Within(1e-10));
                Assert.That(resampled, Is.Not.SameAs(signal));
            });
        }
    }
}
