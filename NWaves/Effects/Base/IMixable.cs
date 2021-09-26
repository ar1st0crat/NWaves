namespace NWaves.Effects.Base
{
    /// <summary>
    /// Interface for wet/dry mixers.
    /// </summary>
    public interface IMixable
    {
        /// <summary>
        /// Gets or sets wet gain.
        /// </summary>
        float Wet { get; set; }

        /// <summary>
        /// Gets or sets dry gain.
        /// </summary>
        float Dry { get; set; }
    }
}
