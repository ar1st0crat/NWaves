using System;

namespace NWaves.Features
{
    /// <summary>
    /// Provides methods for computing perceptual audio features.
    /// </summary>
    public static class Perceptual
    {
        /// <summary>
        /// Computes perceptual loudness (the sum of specific loudnesses: N'(z) = E(z)^0.23).
        /// </summary>
        /// <param name="spectralBands">Array of energies in given spectral bands</param>
        public static float Loudness(float[] spectralBands)
        {
            var loudness = 0.0;

            for (var i = 0; i < spectralBands.Length; i++)
            {
                loudness += Math.Pow(spectralBands[i], 0.23);
            }

            return (float)loudness;
        }

        /// <summary>
        /// <para>Computes perceptual sharpness (essentially, the equivalent of spectral centroid).</para>
        /// </summary>
        /// <param name="spectralBands">Array of energies in given spectral bands</param>
        public static float Sharpness(float[] spectralBands)
        {
            // According to the original formula, the weights are slightly different 
            // for bark bands with index >= 15, but this implementation assumes 
            // that there will be no more than 15 bands.
             
            var sharpness = 0.0;
            var total = 0.0;

            for (var i = 0; i < spectralBands.Length; i++)
            {
                var loudness = Math.Pow(spectralBands[i], 0.23);
                sharpness += (i + 1) * loudness;
                total += loudness;
            }

            return (float)(sharpness / total * 0.11);
        }
    }
}
