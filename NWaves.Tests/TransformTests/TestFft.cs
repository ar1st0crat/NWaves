using NUnit.Framework;
using NWaves.Transforms;
using System;

namespace NWaves.Tests.TransformTests
{
    [TestFixture]
    public class TestFft
    {
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
    }
}
