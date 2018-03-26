using System;
using NUnit.Framework;
using NWaves.Signals;

namespace NWaves.Tests.SignalTests
{
    [TestFixture]
    public class TestComplexDiscreteSignal
    {
        // Arrange
        private readonly ComplexDiscreteSignal _signal = new ComplexDiscreteSignal
            (8000, new double[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 
                   new double[] { 0, 0, 1, 0, 0, 1, 0, 0, 1, 0 });

        private readonly ComplexDiscreteSignal _constant = new ComplexDiscreteSignal
            (8000, new double[] { 5, 5, 5, 5, 5 },
                   new double[] { 1, 1, 1, 1, 1 });

        private readonly ComplexDiscreteSignal _small = new ComplexDiscreteSignal
            (16000, new double[] { 5, 2, 4 },
                    new double[] { 3, 0, 8 });

        [Test]
        public void TestInitializeWithBadSamplingRate()
        {
            Assert.Throws<ArgumentException>(() => { var s = new ComplexDiscreteSignal(0, new double[] {1}); });
            Assert.Throws<ArgumentException>(() => { var s = new ComplexDiscreteSignal(-8000, new double[] { 1 }); });
        }

        [Test]
        public void TestInitializeWithDifferentRealAndImagSizes()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var s = new ComplexDiscreteSignal(8000, new double[] { 1, 2 }, new double[] {3});
            });
        }

        [Test]
        public void TestPositiveDelay()
        {
            //Act
            var delayed = _signal.Delay(3);

            //Assert
            Assert.That(delayed.Real, Is.EqualTo(new double[] { 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
            Assert.That(delayed.Imag, Is.EqualTo(new double[] { 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0 }));
        }

        [Test]
        public void TestNegativeDelay()
        {
            //Act
            var delayed = _signal.Delay(-3);

            //Assert
            Assert.That(delayed.Real, Is.EqualTo(new double[] { 3, 4, 5, 6, 7, 8, 9 }));
            Assert.That(delayed.Imag, Is.EqualTo(new double[] { 0, 0, 1, 0, 0, 1, 0 }));
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
            Assert.That(combination.Real, Is.EqualTo(new double[] { 5, 6, 7, 8, 9, 5, 6, 7, 8, 9 }));
            Assert.That(combination.Imag, Is.EqualTo(new double[] { 1, 1, 2, 1, 1, 1, 0, 0, 1, 0 }));
        }

        [Test]
        public void TestSuperimposeBroadcastObject()
        {
            //Act
            var combination1 = _constant.Superimpose(_signal);
            var combination2 = _constant + _signal;

            //Assert
            Assert.That(combination1.Real, Is.EqualTo(new double[] { 5, 6, 7, 8, 9, 5, 6, 7, 8, 9 }));
            Assert.That(combination1.Imag, Is.EqualTo(new double[] { 1, 1, 2, 1, 1, 1, 0, 0, 1, 0 }));
            Assert.That(combination2.Real, Is.EqualTo(new double[] { 5, 6, 7, 8, 9, 5, 6, 7, 8, 9 }));
            Assert.That(combination2.Imag, Is.EqualTo(new double[] { 1, 1, 2, 1, 1, 1, 0, 0, 1, 0 }));
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
            var concatenation = _signal.Concatenate(_constant);
            
            //Assert
            Assert.That(concatenation.Real, Is.EqualTo(new double[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 5, 5, 5, 5, 5 }));
            Assert.That(concatenation.Imag, Is.EqualTo(new double[] { 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 1, 1, 1, 1, 1 }));
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
            Assert.That(repeated1.Real, Is.EqualTo(new double[] { 5, 2, 4, 5, 2, 4, 5, 2, 4 }));
            Assert.That(repeated1.Imag, Is.EqualTo(new double[] { 3, 0, 8, 3, 0, 8, 3, 0, 8}));
            Assert.That(repeated2.Real, Is.EqualTo(new double[] { 5, 2, 4, 5, 2, 4, 5, 2, 4 }));
            Assert.That(repeated2.Imag, Is.EqualTo(new double[] { 3, 0, 8, 3, 0, 8, 3, 0, 8 }));
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
            Assert.That(slice.Real, Is.EqualTo(new double[] { 3, 4, 5, 6 }));
            Assert.That(slice.Imag, Is.EqualTo(new double[] { 0, 0, 1, 0 }));
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
            Assert.That(first.Real, Is.EqualTo(new double[] { 0, 1, 2 }));
            Assert.That(first.Imag, Is.EqualTo(new double[] { 0, 0, 1 }));
        }

        [Test]
        public void TestLastSamples()
        {
            // Act
            var last = _signal.Last(3);

            // Assert
            Assert.That(last.Real, Is.EqualTo(new double[] { 7, 8, 9 }));
            Assert.That(last.Imag, Is.EqualTo(new double[] { 0, 1, 0 }));
        }

        [Test]
        public void TestNegativeFirstOrLast()
        {
            Assert.Throws<ArgumentException>(() => { var first = _signal.First(-3); });
            Assert.Throws<ArgumentException>(() => { var last = _signal.Last(-5); });
        }
    }
}
