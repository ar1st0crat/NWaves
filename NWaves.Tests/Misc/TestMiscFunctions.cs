using NUnit.Framework;
using NWaves.Utils;

namespace NWaves.Tests.Misc
{
    [TestFixture]
    public class TestMiscFunctions
    {
        [Test]
        public void TestNextPowerOfTwo()
        {
            Assert.That(MathUtils.NextPowerOfTwo(64), Is.EqualTo(64));
            Assert.That(MathUtils.NextPowerOfTwo(63), Is.EqualTo(64));
            Assert.That(MathUtils.NextPowerOfTwo(33), Is.EqualTo(64));
            Assert.That(MathUtils.NextPowerOfTwo(65000), Is.EqualTo(65536));
        }
    }
}
