namespace NWaves.Effects.Base
{
    public interface IMixable
    {
        /// <summary>
        /// Wet gain
        /// </summary>
        float Wet { get; set; }

        /// <summary>
        /// Dry gain
        /// </summary>
        float Dry { get; set; }
    }
}
