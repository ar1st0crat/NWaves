using NUnit.Framework;
using NWaves.Utils;

namespace NWaves.Tests.UtilTests
{
    [TestFixture]
    public class TestScaleFunctions
    {
        [Test]
        public void TestConvertToDecibel()
        {
            Assert.That(Scale.ToDecibel(10), Is.EqualTo(20).Within(1e-10));
            Assert.That(Scale.ToDecibel(100), Is.EqualTo(40).Within(1e-10));
        }

        [Test]
        public void TestConvertFromDecibel()
        {
            Assert.That(Scale.FromDecibel(20), Is.EqualTo(10).Within(1e-10));
            Assert.That(Scale.FromDecibel(40), Is.EqualTo(100).Within(1e-10));
        }

        [Test]
        public void TestConvertToDecibelPower()
        {
            Assert.That(Scale.ToDecibelPower(10), Is.EqualTo(10).Within(1e-10));
            Assert.That(Scale.ToDecibelPower(100), Is.EqualTo(20).Within(1e-10));
        }

        [Test]
        public void TestConvertFromDecibelPower()
        {
            Assert.That(Scale.FromDecibelPower(10), Is.EqualTo(10).Within(1e-10));
            Assert.That(Scale.FromDecibelPower(20), Is.EqualTo(100).Within(1e-10));
        }

        [Test]
        public void TestConvertHerzToMel()
        {
            Assert.That(Scale.HerzToMel(5000), Is.EqualTo(2363.5).Within(0.1));
        }

        [Test]
        public void TestConvertMelToHerz()
        {
            Assert.That(Scale.MelToHerz(2363.5), Is.EqualTo(5000).Within(0.1));
        }

        [Test]
        public void TestConvertHerzToBark()
        {
            Assert.That(Scale.HerzToBark(5000), Is.EqualTo(18.73).Within(0.1));
        }

        [Test]
        public void TestConvertBarkToHerz()
        {
            Assert.That(Scale.BarkToHerz(18.73), Is.EqualTo(5000).Within(0.1));
        }

        [Test]
        public void TestConvertPitchToFreq()
        {
            Assert.That(Scale.PitchToFreq(60), Is.EqualTo(261.626).Within(1e-3));
        }

        [Test]
        public void TestConvertFreqToPitch()
        {
            Assert.That(Scale.FreqToPitch(261.626), Is.EqualTo(60));
        }
    }
}
