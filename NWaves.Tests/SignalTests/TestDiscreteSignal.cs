using System;
using NUnit.Framework;
using NWaves.Signals;

namespace NWaves.Tests.SignalTests
{
    [TestFixture]
    public class TestDiscreteSignal
    {
        private readonly DiscreteSignal _signal = 
            new DiscreteSignal(8000, new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

        private readonly DiscreteSignal _constant = 
            new DiscreteSignal(8000, new float[] { 5, 5, 5, 5, 5 });

        private readonly DiscreteSignal _small = 
            new DiscreteSignal(16000, new float[] { 5, 2, 4 });

        [Test]
        public void TestInitializeWithBadSamplingRate()
        {
            Assert.Throws<ArgumentException>(() => { var s = new ComplexDiscreteSignal(0, new double[] { 1 }); });
            Assert.Throws<ArgumentException>(() => { var s = new ComplexDiscreteSignal(-8000, new double[] { 1 }); });
        }

        [Test]
        public void TestInitializeWithDifferentRealAndImagSizes()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var s = new ComplexDiscreteSignal(8000, new double[] { 1, 2 }, new double[] { 3 });
            });
        }

        [Test]
        public void TestPositiveDelay()
        {
            //Act
            var delayed = _signal.Delay(3);

            //Assert
            Assert.That(delayed.Samples, Is.EqualTo(new float[] { 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
        }

        [Test]
        public void TestNegativeDelay()
        {
            //Act
            var delayed = _signal.Delay(-3);

            //Assert
            Assert.That(delayed.Samples, Is.EqualTo(new float[] { 3, 4, 5, 6, 7, 8, 9 }));
        }

        [Test]
        public void TestNegativeDelayTooBig()
        {
            Assert.Throws<ArgumentException>(() => { _signal.Delay(-10); });
        }

        [Test]
        public void TestSuperimposeBroadcastArgument()
        {
            //Act
            var combination = _signal.Superimpose(_constant);

            //Assert
            Assert.That(combination.Samples, Is.EqualTo(new float[] { 5, 6, 7, 8, 9, 5, 6, 7, 8, 9 }));
        }

        [Test]
        public void TestSuperimposeBroadcastObject()
        {
            //Act
            var combination = _constant.Superimpose(_signal);

            //Assert
            Assert.That(combination.Samples, Is.EqualTo(new float[] { 5, 6, 7, 8, 9, 5, 6, 7, 8, 9 }));
        }

        [Test]
        public void TestSuperimposeWithDifferentSamplingRates()
        {
            Assert.Throws<ArgumentException>(() => { _signal.Superimpose(_small); });
        }

        [Test]
        public void TestConcatenate()
        {
            //Act
            var concatenation1 = _signal.Concatenate(_constant);
            var concatenation2 = _constant + _signal;

            //Assert
            Assert.That(concatenation1.Samples, Is.EqualTo(new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 5, 5, 5, 5, 5 }));
            Assert.That(concatenation2.Samples, Is.EqualTo(new float[] { 5, 5, 5, 5, 5, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
        }

        [Test]
        public void TestConcatenateWithDifferentSamplingRates()
        {
            Assert.Throws<ArgumentException>(() => { _signal.Concatenate(_small); });
        }

        [Test]
        public void TestRepeat()
        {
            //Act
            var repeated1 = _small.Repeat(3);
            var repeated2 = _small * 3;

            //Assert
            Assert.That(repeated1.Samples, Is.EqualTo(new float[] { 5, 2, 4, 5, 2, 4, 5, 2, 4 }));
            Assert.That(repeated2.Samples, Is.EqualTo(new float[] { 5, 2, 4, 5, 2, 4, 5, 2, 4 }));
        }

        [Test]
        public void TestRepeatNegativeTimes()
        {
            Assert.Throws<ArgumentException>(() => { _signal.Repeat(-2); });
        }

        [Test]
        public void TestRepeatZeroTimes()
        {
            Assert.Throws<ArgumentException>(() => { var repeated = _signal * 0; });
        }

        [Test]
        public void TestSlice()
        {
            // Act
            var slice = _signal[3, 7];

            // Assert
            Assert.That(slice.Samples, Is.EqualTo(new float[] { 3, 4, 5, 6 }));
        }

        [Test]
        public void TestSliceIndexOutOfRange()
        {
            Assert.Throws<ArgumentException>(() => { var slice = _signal[5, 15]; });
        }

        [Test]
        public void TestSliceWrongRange()
        {
            Assert.Throws<ArgumentException>(() => { var slice = _signal[5, 5]; });
            Assert.Throws<ArgumentException>(() => { var slice = _signal[5, 4]; });
        }

        [Test]
        public void TestFirstSamples()
        {
            // Act
            var first = _signal.First(3);

            // Assert
            Assert.That(first.Samples, Is.EqualTo(new float[] { 0, 1, 2 }));
        }

        [Test]
        public void TestLastSamples()
        {
            // Act
            var last = _signal.Last(3);

            // Assert
            Assert.That(last.Samples, Is.EqualTo(new float[] { 7, 8, 9 }));
        }

        [Test]
        public void TestNegativeFirstOrLast()
        {
            Assert.Throws<ArgumentException>(() => { var first = _signal.First(-3); });
            Assert.Throws<ArgumentException>(() => { var last = _signal.Last(-5); });
        }
    }
}
