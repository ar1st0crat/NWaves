using NWaves.Effects.Base;
using NWaves.Signals;
using System;

namespace NWaves.Effects.Stereo
{
    /// <summary>
    /// Abstract class for stereo audio effects.
    /// </summary>
    public abstract class StereoEffect : WetDryMixer
    {
        /// <summary>
        /// Processes one sample in each of two channels : [ input left , input right ] -> [ output left , output right ].
        /// </summary>
        /// <param name="left">Input sample in left channel</param>
        /// <param name="right">Input sample in right channel</param>
        public abstract void Process(ref float left, ref float right);

        /// <summary>
        /// Processes one sample in mono channel : input sample -> [ output left , output right ].
        /// </summary>
        /// <param name="sample">Input sample in mono channel</param>
        /// <param name="left">Output sample for left channel</param>
        /// <param name="right">Output sample for right channel</param>
        public virtual void Process(float sample, out float left, out float right)
        {
            left = right = sample;

            Process(ref left, ref right);
        }

        /// <summary>
        /// Processes blocks of samples in each of two channels : [ input left , input right ] -> [ output left , output right ].
        /// </summary>
        /// <param name="inputLeft">Input block of samples (left channel)</param>
        /// <param name="inputRight">Input block of samples (right channel)</param>
        /// <param name="outputLeft">Output block of samples (left channel)</param>
        /// <param name="outputRight">Output block of samples (right channel)</param>
        /// <param name="count">Number of samples to process</param>
        /// <param name="inputPos">Input starting index</param>
        /// <param name="outputPos">Output starting index</param>
        public virtual void Process(float[] inputLeft,
                                    float[] inputRight,
                                    float[] outputLeft,
                                    float[] outputRight,
                                    int count = 0,
                                    int inputPos = 0,
                                    int outputPos = 0)
        {
            if (count <= 0)
            {
                count = Math.Min(inputLeft.Length, inputRight.Length);
            }

            var endPos = inputPos + count;

            for (int n = inputPos, m = outputPos; n < endPos; n++, m++)
            {
                outputLeft[m] = inputLeft[n];
                outputRight[m] = inputRight[n];

                Process(ref outputLeft[m], ref outputRight[m]);
            }
        }

        /// <summary>
        /// Processes block of samples in mono channel : [ input ] -> [ output left , output right ].
        /// </summary>
        /// <param name="input">Input block of samples (mono channel)</param>
        /// <param name="outputLeft">Output block of samples (left channel)</param>
        /// <param name="outputRight">Output block of samples (right channel)</param>
        /// <param name="count">Number of samples to process</param>
        /// <param name="inputPos">Input starting index</param>
        /// <param name="outputPos">Output starting index</param>
        public virtual void Process(float[] input,
                                    float[] outputLeft,
                                    float[] outputRight,
                                    int count = 0,
                                    int inputPos = 0,
                                    int outputPos = 0)
        {
            if (count <= 0)
            {
                count = input.Length;
            }

            var endPos = inputPos + count;

            for (int n = inputPos, m = outputPos; n < endPos; n++, m++)
            {
                outputLeft[m] = outputRight[m] = input[n];

                Process(ref outputLeft[m], ref outputRight[m]);
            }
        }

        /// <summary>
        /// Applies effect to entire <paramref name="signal"/> and returns tuple of output signals [left signal, right signal].
        /// </summary>
        /// <param name="signal">Input signal</param>
        public virtual (DiscreteSignal, DiscreteSignal) ApplyTo(DiscreteSignal signal)
        {
            var sr = signal.SamplingRate;

            var left = new float[signal.Length];
            var right = new float[signal.Length];
            
            Process(signal.Samples, left, right);

            return (new DiscreteSignal(sr, left), new DiscreteSignal(sr, right));
        }

        /// <summary>
        /// Applies effect to entire signals (in left and right channels) 
        /// and returns tuple of output signals [left signal, right signal].
        /// </summary>
        /// <param name="leftSignal">Input signal (left channel)</param>
        /// <param name="rightSignal">Input signal (right channel)</param>
        public virtual (DiscreteSignal, DiscreteSignal) ApplyTo(DiscreteSignal leftSignal, DiscreteSignal rightSignal)
        {
            var srl = leftSignal.SamplingRate;
            var srr = rightSignal.SamplingRate;

            var left = new float[leftSignal.Length];
            var right = new float[rightSignal.Length];

            Process(leftSignal.Samples, rightSignal.Samples, left, right);

            return (new DiscreteSignal(srl, left), new DiscreteSignal(srr, right));
        }

        /// <summary>
        /// Resets effect.
        /// </summary>
        public abstract void Reset();
    }
}
