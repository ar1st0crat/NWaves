using NUnit.Framework;
using NWaves.Transforms;

namespace NWaves.Tests.TransformTests
{
    [TestFixture]
    public class TestCepstrum
    {
        private CepstralTransform _ct = new CepstralTransform(8, 16);

        private float[] _input = new[] { 1, 5, 2, 30, 110, 60, 205, 1, 2, 30f };

        [Test]
        public void TestComplexCepstrum()
        {
            var output = new float[8];

            _ct.DirectNorm(_input, output);

            Assert.That(output, Is.EqualTo(new[] { 5.34123949e+00, -6.07112628e-02, -3.81738790e-02,  1.43294733e-01,
                                                   6.28384672e-03,  1.79680255e-03, -5.12632421e-03,  7.09771400e-03 }).Within(1e-5));
        }

        [Test]
        public void TestInverseComplexCepstrum()
        {
            var ct = new CepstralTransform(8);

            var input = new[] { 1, 7, 2, 100, 59, 32, 11, 72f };
            var output = new float[8];
            var cepstrum = new float[8];

            var d = ct.ComplexCepstrum(input, output);
            ct.InverseComplexCepstrum(output, cepstrum, true, d);

            Assert.That(cepstrum, Is.EqualTo(input).Within(1e-4));
        }

        [Test]
        public void TestRealCepstrum()
        {
            var output = new float[8];

            _ct.RealCepstrum(_input, output);

            Assert.That(output, Is.EqualTo(new[] { 5.34123949e+00,  1.31323203e-01,  2.21040651e-01,  6.34689386e-02,
                                                  -5.90905261e-02,  1.75205402e-02,  1.65489707e-02, -4.19748362e-03 }).Within(1e-5));
        }
    }
}
