using NUnit.Framework;
using NWaves.Operations;
using NWaves.Signals.Builders;

namespace NWaves.Tests.OperationTests
{
    [TestFixture]
    public class TestResampling
    {
        [Test]
        public void TestDecimation()
        {
            var sinusoid = new SinusoidBuilder()
                                    .SetParameter("amp", 1)
                                    .SetParameter("freq", 100)
                                    .SampledAt(16000)
                                    .OfLength(200)
                                    .Build();

            var decimated = Operation.Decimate(sinusoid, 2);

            // TODO:
            //Assert
        }
    }
}
