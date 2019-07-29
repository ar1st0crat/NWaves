using NWaves.Effects;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;

namespace NWaves.DemoUwpEffect
{
    /// <summary>
    /// Adapter pattern:
    /// 
    ///     1) aggregate audio effect from NWaves lib
    ///     2) implement IBasicAudioEffect interface and see UWP sample code
    ///     
    /// </summary>
    public sealed class NWavesEffect : IBasicAudioEffect
    {
        // aggregate AutoWah effect

        AutowahEffect autowahEffect;

        // everythin else is adopted from UWP code samples:

        private AudioEncodingProperties currentEncodingProperties;
        private List<AudioEncodingProperties> supportedEncodingProperties;

        private IPropertySet propertySet;

        private float MaxFrequency
        {
            get
            {
                if (propertySet != null && propertySet.TryGetValue("Max frequency", out object val))
                {
                    return (float)val;
                }
                return 1;
            }
        }

        private float Q
        {
            get
            {
                if (propertySet != null && propertySet.TryGetValue("Q", out object val))
                {
                    return (float)val;
                }
                return 0.5f;
            }
        }

        public bool UseInputFrameForOutput => false;

        /// <summary>
        /// Set up constant members in the constructor
        /// </summary>
        public NWavesEffect()
        {
            // Support 16kHz, 22.05kHz, 44.1kHz and 48kHz mono float

            supportedEncodingProperties = new List<AudioEncodingProperties>();
            AudioEncodingProperties encodingProps1 = AudioEncodingProperties.CreatePcm(16000, 1, 32);
            encodingProps1.Subtype = MediaEncodingSubtypes.Float;
            AudioEncodingProperties encodingProps2 = AudioEncodingProperties.CreatePcm(22050, 1, 32);
            encodingProps2.Subtype = MediaEncodingSubtypes.Float;
            AudioEncodingProperties encodingProps3 = AudioEncodingProperties.CreatePcm(44100, 1, 32);
            encodingProps3.Subtype = MediaEncodingSubtypes.Float;
            AudioEncodingProperties encodingProps4 = AudioEncodingProperties.CreatePcm(48000, 1, 32);
            encodingProps4.Subtype = MediaEncodingSubtypes.Float;

            supportedEncodingProperties.Add(encodingProps1);
            supportedEncodingProperties.Add(encodingProps2);
            supportedEncodingProperties.Add(encodingProps3);
            supportedEncodingProperties.Add(encodingProps4);
        }

        public IReadOnlyList<AudioEncodingProperties> SupportedEncodingProperties => supportedEncodingProperties;

        public void SetEncodingProperties(AudioEncodingProperties encodingProperties)
        {
            currentEncodingProperties = encodingProperties;

            autowahEffect = new AutowahEffect((int)encodingProperties.SampleRate, maxFrequency: MaxFrequency, q: Q)
            {
                Wet = 0.8f,
                Dry = 0.4f
            };
        }

        unsafe public void ProcessFrame(ProcessAudioFrameContext context)
        {
            var q = Q;
            var maxFrequency = MaxFrequency;

            if (maxFrequency != autowahEffect.MaxFrequency)
            {
                autowahEffect.MaxFrequency = maxFrequency;
            }
            if (q != autowahEffect.Q)
            {
                autowahEffect.Q = q;
            }

            AudioFrame inputFrame = context.InputFrame;
            AudioFrame outputFrame = context.OutputFrame;

            using (AudioBuffer inputBuffer = inputFrame.LockBuffer(AudioBufferAccessMode.Read),
                               outputBuffer = outputFrame.LockBuffer(AudioBufferAccessMode.Write))
            using (IMemoryBufferReference inputReference = inputBuffer.CreateReference(),
                                          outputReference = outputBuffer.CreateReference())
            {
                ((IMemoryBufferByteAccess)inputReference).GetBuffer(out byte* inputDataInBytes, out uint inputCapacity);
                ((IMemoryBufferByteAccess)outputReference).GetBuffer(out byte* outputDataInBytes, out uint outputCapacity);

                float* inputDataInFloat = (float*)inputDataInBytes;
                float* outputDataInFloat = (float*)outputDataInBytes;

                int dataInFloatLength = (int)inputBuffer.Length / sizeof(float);

                for (int i = 0; i < dataInFloatLength; i++)
                {
                    outputDataInFloat[i] = autowahEffect.Process(inputDataInFloat[i]);
                }
            }
        }

        public void Close(MediaEffectClosedReason reason)
        {
        }

        public void DiscardQueuedFrames()
        {
            autowahEffect.Reset();
        }

        public void SetProperties(IPropertySet configuration)
        {
            propertySet = configuration;
        }
    }

    // Using the COM interface IMemoryBufferByteAccess allows us to access the underlying byte array in an AudioFrame
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }
}
