using NUnit.Framework;
using NWaves.Transforms;
using System;
using System.Linq;

namespace NWaves.Tests.TransformTests
{
    [TestFixture]
    public class TestFft
    {
        [Test]
        public void TestRealFft()
        {
            float[] array = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

            float[] re = new float[9];
            float[] im = new float[9];

            var realFft = new RealFft(16);

            realFft.Direct(array, re, im);

            Assert.That(re, Is.EqualTo(new float[] { 120, -8, -8, -8, -8, -8, -8, -8, -8 }).Within(1e-5));
            Assert.That(im, Is.EqualTo(new float[] { 0, 40.21872f, 19.31371f, 11.97285f, 8, 5.34543f, 3.31371f, 1.591299f, 0 }).Within(1e-5));
        }

        [Test]
        public void TestInverseRealFft()
        {
            float[] array = { 1, 5, 3, 7, 2, 3, 0, 7 };
            float[] output = new float[array.Length];
            float[] outputNorm = new float[array.Length];

            float[] re = new float[5];
            float[] im = new float[5];

            var realFft = new RealFft(8);

            realFft.Direct(array, re, im);
            realFft.Inverse(re, im, output);
            realFft.InverseNorm(re, im, outputNorm);

            Assert.Multiple(() =>
            {
                Assert.That(output, Is.EqualTo(array.Select(a => a * 4)).Within(1e-5));
                Assert.That(outputNorm, Is.EqualTo(array).Within(1e-5));
            });
        }

        [Test]
        public void TestInverseFftNormalized()
        {
            float[] re = { 1, 5, 3, 7, 2, 3, 0, 7 };
            float[] im = new float[re.Length];

            var fft = new Fft(8);

            fft.Direct(re, im);
            fft.InverseNorm(re, im);

            Assert.That(re, Is.EqualTo(new[] { 1, 5, 3, 7, 2, 3, 0, 7 }).Within(1e-5));
        }

        [Test]
        public void TestFftShift()
        {
            float[] array = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

            Fft.Shift(array);

            Assert.That(array, Is.EqualTo(new float[] { 8, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7 }));
        }

        [Test]
        public void TestFftShiftOddSize()
        {
            float[] array = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

            Assert.Throws<ArgumentException>(() => Fft.Shift(array));
        }

        [Test]
        public void TestGoertzel()
        {
            float[] array = { 1, 2, 3, 4, 5, 6, 7, 8 };

            var cmpx = new Goertzel(8).Direct(array, 2);

            Assert.Multiple(() =>
            {
                Assert.That(cmpx.Real, Is.EqualTo(-4).Within(1e-6));
                Assert.That(cmpx.Imaginary, Is.EqualTo(4).Within(1e-6));
            });
        }

        [Test]
        public void TestHartley()
        {
            float[] re = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

            var dht = new HartleyTransform(16);

            dht.Direct(re);
            dht.Inverse(re);

            Assert.That(re, Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16f }).Within(1e-4));
        }
    }
}
