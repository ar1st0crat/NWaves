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
            var sinusoid = new SinusoidBuilder()
                                    .SetParameter("freq", 0.05)
                                    .OfLength(20)
                                    .Build();

            Assert.Multiple(() =>
            {
                Assert.That(sinusoid[0], Is.EqualTo(0.0).Within(1e-10));
                Assert.That(sinusoid[5], Is.EqualTo(1.0).Within(1e-10));
                Assert.That(sinusoid[10], Is.EqualTo(0.0).Within(1e-10));
                Assert.That(sinusoid[15], Is.EqualTo(-1.0).Within(1e-10));
                Assert.That(sinusoid.Length, Is.EqualTo(20));
            });
        }

        [Test]
        public void TestBuilderSuperimpose()
        {
            var constants = new DiscreteSignal(1, length: 6, value: 2.0);

            var sinusoid = new SinusoidBuilder()
                                    .SetParameter("freq", 0.05)
                                    .SuperimposedWith(constants)
                                    .OfLength(20)
                                    .SuperimposedWith(constants)    // twice
                                    .Build();

            Assert.Multiple(() =>
            {
                Assert.That(sinusoid[0], Is.EqualTo(4.0).Within(1e-10));
                Assert.That(sinusoid[5], Is.EqualTo(5.0).Within(1e-10));
                Assert.That(sinusoid[10], Is.EqualTo(0.0).Within(1e-10));
                Assert.That(sinusoid[15], Is.EqualTo(-1.0).Within(1e-10));
                Assert.That(sinusoid.Length, Is.EqualTo(20));
            });
        }

        [Test]
        public void TestBuilderRepeat()
        {
            var sinusoid = new SinusoidBuilder()
                                    .SetParameter("freq", 0.05)
                                    .OfLength(20)
                                    .RepeatedTimes(3)
                                    .Build();

            Assert.Multiple(() =>
            {
                Assert.That(sinusoid[0], Is.EqualTo(0.0).Within(1e-10));
                Assert.That(sinusoid[5], Is.EqualTo(1.0).Within(1e-10));
                Assert.That(sinusoid[25], Is.EqualTo(1.0).Within(1e-10));
                Assert.That(sinusoid.Length, Is.EqualTo(60));
            });
        }

        [Test]
        public void TestBuilderDelay()
        {
            var sinusoid = new SinusoidBuilder()
                                    .SetParameter("freq", 0.05)
                                    .OfLength(20)
                                    .DelayedBy(1)
                                    .Build();

            Assert.Multiple(() =>
            {
                Assert.That(sinusoid[1], Is.EqualTo(0.0).Within(1e-10));
                Assert.That(sinusoid[6], Is.EqualTo(1.0).Within(1e-10));
                Assert.That(sinusoid[16], Is.EqualTo(-1.0).Within(1e-10));
                Assert.That(sinusoid.Length, Is.EqualTo(21));
            });
        }
    }
}
