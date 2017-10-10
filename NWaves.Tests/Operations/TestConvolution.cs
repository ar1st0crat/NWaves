using NUnit.Framework;
using NWaves.Operations;
using NWaves.Signals;

namespace NWaves.Tests.Operations
{
    [TestFixture]
    public class TestConvolution
    {
        private readonly DiscreteSignal _s1 = new DiscreteSignal(8000, new double[] { 1, 5, 3, 2, 6, 2, 1 });
        private readonly DiscreteSignal _s2 = new DiscreteSignal(8000, new double[] { 2, 3, 1 });

        [Test]
        public void TestFftConvolution()
        {
            var conv = Operation.Convolve(_s1, _s2);

            Assert.That(conv.Samples, Is.EqualTo(new[] { 2, 13, 22, 18, 21, 24, 14, 5, 1.0 }).Within(1e-12));
        }

        [Test]
        public void TestDirectConvolution()
        {
            var conv = Operation.ConvolveDirect(_s1, _s2);

            Assert.That(conv.Samples, Is.EqualTo(new[] { 2, 13, 22, 18, 21, 24, 14, 5, 1.0 }).Within(1e-12));
        }

        [Test]
        public void TestFftCrossCorrelation()
        {
            var conv = Operation.CrossCorrelate(_s1, _s2);

            Assert.That(conv.Samples, Is.EqualTo(new[] { 1, 8, 20, 21, 18, 24, 19, 7, 2.0 }).Within(1e-12));
        }

        [Test]
        public void TestDirectCrossCorrelation()
        {
            var conv = Operation.CrossCorrelateDirect(_s1, _s2);

            Assert.That(conv.Samples, Is.EqualTo(new[] { 1, 8, 20, 21, 18, 24, 19, 7, 2.0 }).Within(1e-12));
        }
    }
}
