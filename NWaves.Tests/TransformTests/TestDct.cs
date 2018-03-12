using NUnit.Framework;
using NWaves.Transforms;

namespace NWaves.Tests.TransformTests
{
    [TestFixture]
    public class TestDct
    {
        readonly double[] _test = { 0.5, 0.2, 0.1, 0.6, 0.1, 0, 0.3, 0.8 };

        [Test]
        public void TestDct1()
        {
            double[] res = new double[6];
            double[] resDct1 = { 1.95, -0.06648744, 0.3088146, -0.57409388, 0.88508551, 0.34058132};

            var dct1 = new Dct1(8, 6);
            dct1.Direct(_test, res);

            Assert.That(res, Is.EqualTo(resDct1).Within(1e-5));
        }

        [Test]
        public void TestDct2()
        {
            double[] res = new double[6];
            double[] resDct2 = { 2.6, -0.22428036, 0.70740109, -0.6057955, 0.98994949, 0.3666513};

            var dct2 = new Dct2(8, 6);
            dct2.Direct(_test, res);

            Assert.That(res, Is.EqualTo(resDct2).Within(1e-5));
        }

        [Test]
        public void TestDct3()
        {
            double[] res = new double[6];
            double[] resDct3 = { 1.37901474, -0.45482265, 0.60600341, -0.9654346, 1.19246999, 0.23036627};

            var dct3 = new Dct3(8, 6);
            dct3.Direct(_test, res);

            Assert.That(res, Is.EqualTo(resDct3).Within(1e-5));
        }

        [Test]
        public void TestIdct1()
        {
            double[] res = new double[8];
            double[] resDct1 = { 1.95, -0.06648744, 0.3088146, -0.57409388, 0.88508551, 0.34058132, 0.10609989, -0.45 };

            var invdct = new Dct1(8, 8);
            invdct.Inverse(resDct1, res);

            Assert.That(res, Is.EqualTo(_test).Within(1e-5));
        }

        [Test]
        public void TestIdct2()
        {
            double[] res = new double[8];
            double[] resDct2 = { 2.6, -0.22428036, 0.70740109, -0.6057955, 0.98994949, 0.3666513, -0.13994175, -0.41021575 };

            var invdct = new Dct2(8, 8);
            invdct.Inverse(resDct2, res);

            Assert.That(res, Is.EqualTo(_test).Within(1e-5));
        }

        [Test]
        public void TestIdct3()
        {
            double[] res = new double[8];
            double[] resDct3 = { 1.37901474, -0.45482265, 0.60600341, -0.9654346, 1.19246999, 0.23036627, 0.33561026, -0.32320742 };

            var invdct = new Dct3(8, 8);
            invdct.Inverse(resDct3, res);

            Assert.That(res, Is.EqualTo(_test).Within(1e-5));
        }
    }
}
