﻿namespace NWaves.Operations
{
    public enum DynamicsMode
    {
        /// <summary>
        /// Smaller ratios, like 1:1, 2:1
        /// </summary>
        Compressor,

        /// <summary>
        /// Bigger ratios, like 5:1, 10:1
        /// </summary>
        Limiter,

        /// <summary>
        /// Smaller ratios, like 1:1, 2:1
        /// </summary>
        Expander,

        /// <summary>
        /// Very high ratios, like 5:1
        /// </summary>
        NoiseGate
    }
}