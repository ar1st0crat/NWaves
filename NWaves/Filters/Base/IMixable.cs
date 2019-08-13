namespace NWaves.Filters.Base
{
    public interface IMixable
    {
        /// <summary>
        /// Wet mix
        /// </summary>
        float Wet { get; set; }

        /// <summary>
        /// Dry mix
        /// </summary>
        float Dry { get; set; }
    }
}
