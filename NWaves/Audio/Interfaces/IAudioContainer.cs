using System.Collections.Generic;
using NWaves.Signals;

namespace NWaves.Audio.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAudioContainer
    {
        /// <summary>
        /// 
        /// </summary>
        List<DiscreteSignal> Signals { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        DiscreteSignal this[Channels channel] { get; }
    }
}
