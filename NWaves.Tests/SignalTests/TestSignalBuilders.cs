using NUnit.Framework;
using NWaves.Signals;
using NWaves.Signals.Builders;

namespace NWaves.Tests.SignalTests
{
    [TestFixture]
    public class TestSignalBuilders
    {
        [Test]
        public void TestSimpleSinusoidBuilder()
        {
            var sinusoid = new SineBuilder()
                                    .SetParameter("freq", 0.05f)
                                    .OfLength(20)
                                    .Build();

            Assert.Multiple(() =>
            {
                Assert.That(sinusoid[0], Is.EqualTo(0.0).Within(1e-7));
                Assert.That(sinusoid[5], Is.EqualTo(1.0).Within(1e-7));
                Assert.That(sinusoid[10], Is.EqualTo(0.0).Within(1e-7));
                Assert.That(sinusoid[15], Is.EqualTo(-1.0).Within(1e-7));
                Assert.That(sinusoid.Length, Is.EqualTo(20));
            });
        }

        [Test]
        public void TestBuilderSuperimpose()
        {
            var constants = new DiscreteSignal(1, length: 6, value: 2.0f);

            var sinusoid = new SineBuilder()
                                    .SetParameter("freq", 0.05f)
                                    .SuperimposedWith(constants)
                                    .OfLength(20)
                                    .SuperimposedWith(constants)    // twice
                                    .Build();

            Assert.Multiple(() =>
            {
                Assert.That(sinusoid[0], Is.EqualTo(4.0).Within(1e-7));
                Assert.That(sinusoid[5], Is.EqualTo(5.0).Within(1e-7));
                Assert.That(sinusoid[10], Is.EqualTo(0.0).Within(1e-7));
                Assert.That(sinusoid[15], Is.EqualTo(-1.0).Within(1e-7));
                Assert.That(sinusoid.Length, Is.EqualTo(20));
            });
        }

        [Test]
        public void TestBuilderRepeat()
        {
            var sinusoid = new SineBuilder()
                                    .SetParameter("freq", 0.05f)
                                    .OfLength(20)
                                    .RepeatedTimes(3)
                                    .Build();

            Assert.Multiple(() =>
            {
                Assert.That(sinusoid[0], Is.EqualTo(0.0).Within(1e-7));
                Assert.That(sinusoid[5], Is.EqualTo(1.0).Within(1e-7));
                Assert.That(sinusoid[25], Is.EqualTo(1.0).Within(1e-7));
                Assert.That(sinusoid.Length, Is.EqualTo(60));
            });
        }

        [Test]
        public void TestBuilderDelay()
        {
            var sinusoid = new SineBuilder()
                                    .SetParameter("freq", 0.05f)
                                    .OfLength(20)
                                    .DelayedBy(1)
                                    .Build();

            Assert.Multiple(() =>
            {
                Assert.That(sinusoid[1], Is.EqualTo(0.0).Within(1e-7));
                Assert.That(sinusoid[6], Is.EqualTo(1.0).Within(1e-7));
                Assert.That(sinusoid[16], Is.EqualTo(-1.0).Within(1e-7));
                Assert.That(sinusoid.Length, Is.EqualTo(21));
            });
        }

        [Test]
        public void TestWavetableBuilder()
        {
            var wavetable = new[] { 1, 2, 3, 4, 5f };
            var wt = new WaveTableBuilder(wavetable).OfLength(7);

            var result = wt.Build();

            Assert.That(result.Samples, Is.EqualTo(new[] { 1, 2, 3, 4, 5, 1, 2f }));
        }
    }
}
