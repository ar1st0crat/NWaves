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
            float[] resDct1 = { 3.9f, -0.13297488f, 0.6176292f, -1.14818776f, 1.77017102f, 0.68116264f };

            var dct1 = new Dct1(8);
            dct1.Direct(_test, res);

            Assert.That(res, Is.EqualTo(resDct1).Within(1e-5));
        }

        [Test]
        public void TestDct1Norm()
        {
            float[] res = new float[6];
            float[] resDct1 = { 0.83879343f, -0.06875f, 0.30898255f, -0.34007706f, 0.6170123f, 0.1488374f };

            var dct1 = new Dct1(8);
            dct1.DirectNorm(_test, res);

            Assert.That(res, Is.EqualTo(resDct1).Within(1e-5));
        }

        [Test]
        public void TestDct2()
        {
            float[] res = new float[6];
            float[] resDct2 = { 5.2f, -0.44856072f, 1.41480218f, -1.21159099f, 1.97989899f, 0.73330259f };

            var dct2 = new Dct2(8);
            dct2.Direct(_test, res);

            Assert.That(res, Is.EqualTo(resDct2).Within(1e-5));
        }

        [Test]
        public void TestDct2Norm()
        {
            float[] res = new float[6];
            float[] resDct2 = { 0.91923882f, -0.11214018f,  0.35370055f, -0.30289775f, 0.49497475f,  0.18332565f };

            var dct2 = new Dct2(8);
            dct2.DirectNorm(_test, res);

            Assert.That(res, Is.EqualTo(resDct2).Within(1e-5));
        }

        [Test]
        public void TestDct3()
        {
            float[] input = { 5.2f,       -0.44856072f,  1.41480218f, -1.21159099f,
                              1.97989899f, 0.73330259f, -0.27988351f, -0.8204315f };
            float[] res = new float[6];
            float[] resDct3 = { 8, 3.2f, 1.6f, 9.6f, 1.6f, 0 };

            var dct3 = new Dct3(8);
            dct3.Direct(input, res);

            Assert.That(res, Is.EqualTo(resDct3).Within(1e-5));
        }

        [Test]
        public void TestDct3Norm()
        {
            float[] res = new float[6];
            float[] resDct3 = { 0.74128407f, -0.17563463f,  0.3547784f , -0.43094061f,  0.64801169f, 0.16695983f };

            var dct3 = new Dct3(8);
            dct3.DirectNorm(_test, res);

            Assert.That(res, Is.EqualTo(resDct3).Within(1e-5));
        }

        [Test]
        public void TestDct4()
        {
            float[] res = new float[6];
            float[] resDct4 = { 2.93983455f, -0.44002102f,  0.91148631f, -0.83446081f,  2.91784085f, -0.99510869f };

            var dct4 = new Dct4(8);
            dct4.Direct(_test, res);

            Assert.That(res, Is.EqualTo(resDct4).Within(1e-5));
        }

        [Test]
        public void TestDct4Norm()
        {
            float[] res = new float[6];
            float[] resDct4 = { 0.73495864f, -0.11000525f, 0.22787158f, -0.2086152f, 0.72946021f, -0.24877717f };

            var dct4 = new Dct4(8);
            dct4.DirectNorm(_test, res);

            Assert.That(res, Is.EqualTo(resDct4).Within(1e-5));
        }

        [Test]
        public void TestIdct1()
        {
            float[] res = new float[6];
            float[] resDct1 = { 3.9f, -0.13297488f, 0.6176292f, -1.14818776f, 1.77017102f, 0.68116264f };

            var dct1 = new Dct1(8);
            dct1.Inverse(_test, res);

            Assert.That(res, Is.EqualTo(resDct1).Within(1e-5));
        }

        [Test]
        public void TestIdct2()
        {
            float[] output = new float[8];
            float[] input = { 5.2f, -0.44856072f, 1.41480218f, -1.21159099f, 1.97989899f, 0.73330259f };
            float[] expected =  { 8.53433006f,  1.77122807f,  3.48148502f,  7.77645215f,
                                   2.99512072f, -0.84717044f,  5.19445736f, 12.69409707f };

            var invdct = new Dct2(8);
            invdct.Inverse(input, output);

            Assert.That(output, Is.EqualTo(expected).Within(1e-5));
        }

        [Test]
        public void TestIdct3()
        {
            float[] res = new float[8];
            float[] resDct3 = { 5.2f,       -0.44856072f,  1.41480218f, -1.21159099f,
                                1.97989899f, 0.73330259f, -0.27988351f, -0.8204315f };

            var invdct = new Dct3(8);
            invdct.Inverse(_test, res);

            Assert.That(res, Is.EqualTo(resDct3).Within(1e-5));
        }

        [Test]
        public void TestIdct4()
        {
            float[] res = new float[6];
            float[] resDct4 = { 2.93983455f, -0.44002102f, 0.91148631f, -0.83446081f, 2.91784085f, -0.99510869f };

            var dct4 = new Dct4(8);
            dct4.Inverse(_test, res);

            Assert.That(res, Is.EqualTo(resDct4).Within(1e-5));
        }

        [Test]
        public void TestMdct()
        {
            float[] res = new float[4];
            float[] fres = new float[4];
            float[] resMdct = { -0.38134489f, -0.01107969f, 0.27565625f, 0.09201603f };

            var mdct = new Mdct(4);
            var fmdct = new FastMdct(4);
            mdct.DirectNorm(_test, res);
            fmdct.DirectNorm(_test, fres);

            Assert.Multiple(() =>
            {
                Assert.That(res, Is.EqualTo(resMdct).Within(1e-5));
                Assert.That(fres, Is.EqualTo(resMdct).Within(1e-5));
            });
        }

        [Test]
        public void TestImdct()
        {
            float[] res = new float[8];
            float[] fres = new float[8];
            float[] resMdct = { -0.38134489f, -0.01107969f, 0.27565625f, 0.09201603f };

            var mdct = new Mdct(4);
            var fmdct = new FastMdct(4);
            mdct.InverseNorm(resMdct, res);
            fmdct.InverseNorm(resMdct, fres);

            Assert.Multiple(() =>
            {
                Assert.That(res, Is.EqualTo(new[] { -0.05f, 0.05f, -0.05f, 0.05f, 0.45f, 0.15f, 0.15f, 0.45f }).Within(1e-5));
                Assert.That(fres, Is.EqualTo(res).Within(1e-5));
            });
        }
    }
}
