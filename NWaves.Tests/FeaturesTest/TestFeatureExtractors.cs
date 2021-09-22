using NUnit.Framework;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Options;
using NWaves.Signals.Builders;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Tests.FeaturesTest
{
    [TestFixture]
    class TestFeatureExtractors
    {
        [Test]
        public void TestOnlineFeatureExtractor()
        {
            var mfccOptions = new MfccOptions
            {
                SamplingRate = 8000,
                FeatureCount = 5,
                FrameSize = 256,
                HopSize = 50,
                FilterBankSize = 8
            };

            var signal = new WhiteNoiseBuilder().OfLength(1000).Build();

            var mfccExtractor = new MfccExtractor(mfccOptions);
            var mfccVectors = mfccExtractor.ComputeFrom(signal);

            var onlineMfccExtractor = new OnlineFeatureExtractor(new MfccExtractor(mfccOptions));
            var onlineMfccVectors = new List<float[]>();

            var i = 0;
            while (i < signal.Length)
            {
                // emulating online blocks with different sizes:
                var size = (i + 1) * 15;
                var block = signal.Samples.Skip(i).Take(size).ToArray();

                var newVectors = onlineMfccExtractor.ComputeFrom(block);

                onlineMfccVectors.AddRange(newVectors);
                
                i += size;
            }

            var diff = mfccVectors.Zip(onlineMfccVectors, (e, o) => e.Zip(o, (f1, f2) => f1 - f2).Sum());

            Assert.That(diff, Is.All.EqualTo(0).Within(1e-7f));
        }
    }
}
