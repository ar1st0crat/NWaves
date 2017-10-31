using NUnit.Framework;
using NWaves.Operations;
using NWaves.Signals;

namespace NWaves.Tests.Operations
{
    [TestFixture]
    public class TestConvolution
    {
        private readonly DiscreteSignal _input = new DiscreteSignal(8000, new double[] { 1, 5, 3, 2, 6, 2, 1 });
        private readonly DiscreteSignal _kernel = new DiscreteSignal(8000, new double[] { 2, 3, 1 });
        private readonly DiscreteSignal _output = new DiscreteSignal(8000, new double[] { 2, 13, 22, 18, 21, 24, 14, 5, 1 });

        [Test]
        public void TestFftConvolution()
        {
            var conv = Operation.Convolve(_input, _kernel);

            Assert.That(conv.Samples, Is.EqualTo(_output.Samples).Within(1e-12));
        }

        [Test]
        public void TestDirectConvolution()
        {
            var conv = Operation.ConvolveDirect(_input, _kernel);

            Assert.That(conv.Samples, Is.EqualTo(_output.Samples).Within(1e-12));
        }

        [Test]
        public void TestFftCrossCorrelation()
        {
            var conv = Operation.CrossCorrelate(_input, _kernel);

            Assert.That(conv.Samples, Is.EqualTo(new[] { 1, 8, 20, 21, 18, 24, 19, 7, 2.0 }).Within(1e-12));
        }

        [Test]
        public void TestDirectCrossCorrelation()
        {
            var conv = Operation.CrossCorrelateDirect(_input, _kernel);

            Assert.That(conv.Samples, Is.EqualTo(new[] { 1, 8, 20, 21, 18, 24, 19, 7, 2.0 }).Within(1e-12));
        }

        [Test]
        public void TestImaginaryParts()
        {
            var s1 = new ComplexDiscreteSignal(1, new[] { 1, 0.5 }, new[] { 0, -1.5 });
            var s2 = new ComplexDiscreteSignal(1, new[] { 1, 0.5 }, new[] { 0, 1.5 });

            var conv = Operation.Convolve(s1, s2);

            Assert.That(conv.Real, Is.EquivalentTo(new[] { 1, 1, 2.5 }));
            Assert.That(conv.Imag, Is.EqualTo(new[] { 0, 0, 0 }).Within(1e-8));
        }
    }
}
