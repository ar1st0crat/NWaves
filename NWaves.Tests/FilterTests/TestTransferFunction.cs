using NUnit.Framework;
using NWaves.Filters;
using NWaves.Filters.Base;

namespace NWaves.Tests.FilterTests
{
    [TestFixture]
    public class TestTransferFunction
    {
        [Test]
        public void TestParallelIir()
        {
            var f1 = new IirFilter(new[] { 1, -0.1 }, new[] { 1,  0.2 });
            var f2 = new IirFilter(new[] { 1,  0.4 }, new[] { 1, -0.6 });

            var f = f1 + f2;

            Assert.Multiple(() =>
            {
                Assert.That(f.Tf.Numerator, Is.EqualTo(new[] { 2, -0.1, 0.14 }).Within(1e-10));
                Assert.That(f.Tf.Denominator, Is.EqualTo(new[] { 1, -0.4, -0.12 }).Within(1e-10));
            });
        }

        [Test]
        public void TestParallelFirIir()
        {
            var f1 = new FirFilter(new[] { 1, -0.1 });
            var f2 = new IirFilter(new[] { 1, 0.4 }, new[] { 1, -0.6 });

            var f = f1 + f2;

            Assert.Multiple(() =>
            {
                Assert.That(f.Tf.Numerator, Is.EqualTo(new[] {2, -0.3, 0.06 }).Within(1e-10));
                Assert.That(f.Tf.Denominator, Is.EqualTo(new[] { 1, -0.6 }).Within(1e-10));
                Assert.That(f, Is.TypeOf<IirFilter>());
            });
        }

        [Test]
        public void TestParallelFirFir()
        {
            var f1 = new FirFilter(new[] { 1, -0.1 });
            var f2 = new FirFilter(new[] { 1, -0.6 });

            var f = f1 + f2;

            Assert.Multiple(() =>
            {
                Assert.That(f.Tf.Numerator, Is.EqualTo(new[] { 2, -0.7 }).Within(1e-10));
                Assert.That(f, Is.TypeOf<FirFilter>());
            });
        }

        [Test]
        public void TestGroupDelay()
        {
            var f = new MovingAverageFilter(5);
            var fr = f.FrequencyResponse();

            Assert.That(fr.GroupDelay, Is.All.EqualTo(2.0).Within(1e-10));
        }
    }
}
