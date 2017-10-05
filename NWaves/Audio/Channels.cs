namespace NWaves.Audio
{
    /// <summary>
    /// Most used channels are: left and right.
    /// Also we add the special case: interleaved channels
    /// </summary>
    public enum Channels
    {
        /// <summary>
        /// Left channel (=0)
        /// </summary>
        Left,

        /// <summary>
        /// Right channel (=1)
        /// </summary>
        Right,

        /// <summary>
        /// Interleaved channels
        /// </summary>
        Interleave = 255
    }
}
