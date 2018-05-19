using System;
using System.Linq;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.Operations
{
    public static partial class Operation
    {
        /// <summary>
        /// Spectral subtraction algorithm according to
        /// 
        /// [1979] M. Berouti, R. Schwartz, J. Makhoul
        /// "Enhancement of Speech Corrupted by Acoustic Noise".
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="noise"></param>
        /// <param name="fftSize"></param>
        /// <param name="hopSize"></param>
        /// <returns></returns>
        public static DiscreteSignal SpectralSubtract(DiscreteSignal signal, 
                                                      DiscreteSignal noise,
                                                      int fftSize = 1024,
                                                      int hopSize = 410)
        {
            var input = signal.Samples;
            var output = new float[input.Length];
            
            const float beta = 0.009f;
            const float alphaMin = 2f;
            const float alphaMax = 5f;
            const float snrMin = -5f;
            const float snrMax = 20f;

            const float k = (alphaMin - alphaMax) / (snrMax - snrMin);
            const float b = alphaMax - k * snrMin;

            var fft = new Fft(fftSize);
            var hannWindow = Window.OfType(WindowTypes.Hann, fftSize);
            var windowSquared = hannWindow.Select(w => w * w).ToArray();
            var windowSum = new float[output.Length];

            var re = new float[fftSize];
            var im = new float[fftSize];
            var zeroblock = new float[fftSize];


            // estimate noise power spectrum

            var noiseAcc = new float[fftSize / 2 + 1];
            var noiseEstimate = new float[fftSize / 2 + 1];

            var numFrames = 0;
            var pos = 0;
            for (; pos + fftSize < noise.Length; pos += hopSize, numFrames++)
            {
                noise.Samples.FastCopyTo(re, fftSize, pos);
                zeroblock.FastCopyTo(im, fftSize);
                
                fft.Direct(re, im);

                for (var j = 0; j <= fftSize/2; j++)
                {
                    noiseAcc[j] += re[j] * re[j] + im[j] * im[j];
                }
            }

            // (including smoothing)

            for (var j = 1; j < fftSize / 2; j++)
            {
                noiseEstimate[j] = (noiseAcc[j - 1] + noiseAcc[j] + noiseAcc[j + 1]) / (3 * numFrames);
            }
            noiseEstimate[0] /= numFrames;
            noiseEstimate[fftSize/2] /= numFrames;


            // spectral subtraction

            for (pos = 0; pos + fftSize < input.Length; pos += hopSize)
            {
                input.FastCopyTo(re, fftSize, pos);
                zeroblock.FastCopyTo(im, fftSize);

                re.ApplyWindow(hannWindow);

                fft.Direct(re, im);

                for (var j = 0; j <= fftSize / 2; j++)
                {
                    var power = re[j] * re[j] + im[j] * im[j];
                    var phase = Math.Atan2(im[j], re[j]);

                    var noisePower = noiseEstimate[j];
                    
                    var snr = 10 * Math.Log10(power / noisePower);
                    var alpha = Math.Max(Math.Min(k * snr + b, alphaMax), alphaMin);

                    var diff = power - alpha * noisePower;
                    
                    var mag = Math.Sqrt(Math.Max(diff, beta * noisePower));
                    
                    re[j] = (float)(mag * Math.Cos(phase));
                    im[j] = (float)(mag * Math.Sin(phase));
                }

                for (var j = fftSize / 2 + 1; j < fftSize; j++)
                {
                    re[j] = im[j] = 0.0f;
                }

                fft.Inverse(re, im);

                for (var j = 0; j < re.Length; j++)
                {
                    output[pos + j] += re[j] * hannWindow[j];
                    windowSum[pos + j] += windowSquared[j];
                }
            }

            for (var j = 0; j < output.Length; j++)
            {
                if (windowSum[j] < 1e-3) continue;
                output[j] /= (windowSum[j] * fftSize / 2);
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }
    }
}
