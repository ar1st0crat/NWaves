using NWaves.Signals.Builders.Base;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// <para>Fade in/out decorator for signal builders.</para>
    /// <para>Example:</para>
    /// <code>
    ///     var sine = new SineBuilder(...); 
    ///     <br/>
    ///     var fadeSine = new FadeInOutBuilder(sine).In(0.05).Out(0.2);
    /// </code>
    /// </summary>
    public class FadeInOutBuilder : SignalBuilder
    {
        /// <summary>
        /// Signal builder to decorate.
        /// </summary>
        private readonly SignalBuilder _builder;

        /// <summary>
        /// Number of samples in fade-in section.
        /// </summary>
        private int _fadeInSampleCount;

        /// <summary>
        /// Number of samples in fade-out section.
        /// </summary>
        private int _fadeOutSampleCount;

        /// <summary>
        /// Index of the sample in fade-in section.
        /// </summary>
        private int _fadeInIndex;

        /// <summary>
        /// Index of the sample in fade-out section.
        /// </summary>
        private int _fadeOutIndex;

        /// <summary>
        /// Index of current sample.
        /// </summary>
        private int _index;

        /// <summary>
        /// Constructs <see cref="FadeInOutBuilder"/> around <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">Underlying signal builder</param>
        public FadeInOutBuilder(SignalBuilder builder)
        {
            _builder = builder;
            _index = 0;
            
            Length = _builder.Length;
            SamplingRate = _builder.SamplingRate;
        }

        /// <summary>
        /// Generate new sample.
        /// </summary>
        public override float NextSample()
        {
            var sample = _builder.NextSample();

            if (FadeStarted || _index++ > Length - _fadeOutSampleCount)
            {
                sample *= (float)_fadeOutIndex-- / _fadeOutSampleCount;

                FadeStarted = !FadeFinished;
            }

            if (_fadeInIndex < _fadeInSampleCount)
            {
                sample *= (float)_fadeInIndex++ / _fadeInSampleCount;
            }

            return sample;
        }

        /// <summary>
        /// Reset sample generator.
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            _index = 0;
            _fadeInIndex = 0;
            _fadeOutIndex = _fadeOutSampleCount - 1;
            FadeStarted = false;
        }

        /// <summary>
        /// Set duration of fade-in section in seconds.
        /// </summary>
        /// <param name="seconds"></param>
        public FadeInOutBuilder In(double seconds)
        {
            _fadeInSampleCount = (int)(seconds * SamplingRate);
            return this;
        }

        /// <summary>
        /// Set duration of fade-out section in seconds.
        /// </summary>
        /// <param name="seconds"></param>
        public FadeInOutBuilder Out(double seconds)
        {
            _fadeOutSampleCount = (int)(seconds * SamplingRate);
            _fadeOutIndex = _fadeOutSampleCount - 1;
            return this;
        }

        /// <summary>
        /// Start fading out.
        /// </summary>
        public void FadeOut()
        {
            if (_fadeOutSampleCount > 0)
            {
                _fadeOutIndex = _fadeOutSampleCount - 1;
                FadeStarted = true;
            }
        }

        /// <summary>
        /// Is signal started fading.
        /// </summary>
        public bool FadeStarted { get; protected set; }

        /// <summary>
        /// Is signal finished fading.
        /// </summary>
        public bool FadeFinished => _fadeOutIndex <= 0;
    }
}
