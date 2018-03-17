using NUnit.Framework;
using NWaves.Transforms;

namespace NWaves.Tests.TransformTests
{
    [TestFixture]
    public class TestDct
    {
        readonly float[] _test = { 0.5f, 0.2f, 0.1f, 0.6f, 0.1f, 0, 0.3f, 0.8f };

        [Test]
        public void TestDct1()
        {
            float[] res = new float[6];
            float[] resDct1 = { 1.95f, -0.06648744f, 0.3088146f, -0.57409388f, 0.88508551f, 0.34058132f};

            var dct1 = new Dct1(8, 6);
            dct1.Direct(_test, res);

            Assert.That(res, Is.EqualTo(resDct1).Within(1e-5));
        }

        [Test]
        public void TestDct2()
        {
            float[] res = new float[6];
            float[] resDct2 = { 2.6f, -0.22428036f, 0.70740109f, -0.6057955f, 0.98994949f, 0.3666513f};

            var dct2 = new Dct2(8, 6);
            dct2.Direct(_test, res);

            Assert.That(res, Is.EqualTo(resDct2).Within(1e-5));
        }

        [Test]
        public void TestDct3()
        {
            float[] res = new float[6];
            float[] resDct3 = { 1.37901474f, -0.45482265f, 0.60600341f, -0.9654346f, 1.19246999f, 0.23036627f};

            var dct3 = new Dct3(8, 6);
            dct3.Direct(_test, res);

            Assert.That(res, Is.EqualTo(resDct3).Within(1e-5));
        }

        [Test]
        public void TestIdct1()
        {
            float[] res = new float[8];
            float[] resDct1 = { 1.95f, -0.06648744f, 0.3088146f, -0.57409388f, 0.88508551f, 0.34058132f, 0.10609989f, -0.45f };

            var invdct = new Dct1(8, 8);
            invdct.Inverse(resDct1, res);

            Assert.That(res, Is.EqualTo(_test).Within(1e-5));
        }

        [Test]
        public void TestIdct2()
        {
            float[] res = new float[8];
            float[] resDct2 = { 2.6f, -0.22428036f, 0.70740109f, -0.6057955f, 0.98994949f, 0.3666513f, -0.13994175f, -0.41021575f };

            var invdct = new Dct2(8, 8);
            invdct.Inverse(resDct2, res);

            Assert.That(res, Is.EqualTo(_test).Within(1e-5));
        }

        [Test]
        public void TestIdct3()
        {
            float[] res = new float[8];
            float[] resDct3 = { 1.37901474f, -0.45482265f, 0.60600341f, -0.9654346f, 1.19246999f, 0.23036627f, 0.33561026f, -0.32320742f };

            var invdct = new Dct3(8, 8);
            invdct.Inverse(resDct3, res);

            Assert.That(res, Is.EqualTo(_test).Within(1e-5));
        }
    }
}
