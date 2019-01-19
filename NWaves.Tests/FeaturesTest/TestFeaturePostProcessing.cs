using NUnit.Framework;
using NWaves.FeatureExtractors.Base;
using System.Collections.Generic;

namespace NWaves.Tests.FeaturesTest
{
    [TestFixture]
    public class TestFeaturePostProcessing
    {
        List<FeatureVector> _feats;

        [SetUp]
        public void InitFeatureVectors()
        {
            _feats = new List<FeatureVector>()
            {
                new FeatureVector { Features = new float[] { 1, 2, 3, 4, 5 } },
                new FeatureVector { Features = new float[] { 0, 2, 4, 0, 4 } },
                new FeatureVector { Features = new float[] { 0, 2, 6, 4, 3 } },
                new FeatureVector { Features = new float[] { 0, 2, 4, 0, 2 } },
                new FeatureVector { Features = new float[] { 0, 2, 3, 4, 1 } },
            };
        }

        [Test]
        public void TestMeanNormalization()
        {
            FeaturePostProcessing.NormalizeMean(_feats);

            Assert.Multiple(() =>
            {
                Assert.That(_feats[0].Features, Is.EqualTo(new float[] {  0.8f, 0, -1,  1.6f,  2 }).Within(1e-5));
                Assert.That(_feats[1].Features, Is.EqualTo(new float[] { -0.2f, 0,  0, -2.4f,  1 }).Within(1e-5));
                Assert.That(_feats[2].Features, Is.EqualTo(new float[] { -0.2f, 0,  2,  1.6f,  0 }).Within(1e-5));
                Assert.That(_feats[3].Features, Is.EqualTo(new float[] { -0.2f, 0,  0, -2.4f, -1 }).Within(1e-5));
                Assert.That(_feats[4].Features, Is.EqualTo(new float[] { -0.2f, 0, -1,  1.6f, -2 }).Within(1e-5));
            });
        }

        [Test]
        public void TestVarianceNormalization()
        {
            FeaturePostProcessing.NormalizeVariance(_feats, bias: 0);

            Assert.Multiple(() =>
            {
                Assert.That(_feats[0].Features, Is.EqualTo(new float[] { 2.5f, 2, 2.74f,  2.04f,  3.536f }).Within(1e-2));
                Assert.That(_feats[1].Features, Is.EqualTo(new float[] {    0, 2, 3.65f,      0,  2.83f  }).Within(1e-2));
                Assert.That(_feats[2].Features, Is.EqualTo(new float[] {    0, 2, 5.48f,  2.04f,  2.12f  }).Within(1e-2));
                Assert.That(_feats[3].Features, Is.EqualTo(new float[] {    0, 2, 3.65f,      0,  1.41f  }).Within(1e-2));
                Assert.That(_feats[4].Features, Is.EqualTo(new float[] {    0, 2, 2.74f,  2.04f,  0.71f  }).Within(1e-2));
            });
        }

        [Test]
        public void TestDeltas()
        {
            FeaturePostProcessing.AddDeltas(_feats);

            Assert.That(_feats[0].Features, Is.EqualTo(
                            new float[] {    1,   2,     3,    4,     5,        // main features
                                          -.3f,   0,   .7f, -.4f,  -.5f,        // delta
                                          .02f,   0, -.16f, .04f, -.13f  })     // delta-delta
                                 .Within(1e-5));

                //Assert.That(_feats[1].Features, Is.EqualTo(
                //                           new float[] {    0,   2,   4,    0,    4,
                //                                         -.3f,   0, .5f, -.8f, -.8f  }).Within(1e-5));
                //Assert.That(_feats[2].Features, Is.EqualTo(
                //                           new float[] {    0,   2,   6,    4,    3,
                //                                         -.2f,   0,   0,    0,   -1  }).Within(1e-5));
        }
    }
}
