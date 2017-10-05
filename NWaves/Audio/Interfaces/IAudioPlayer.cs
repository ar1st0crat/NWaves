using System.Threading.Tasks;
using NWaves.Signals;

namespace NWaves.Audio.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAudioPlayer
    {
        /// <summary>
        /// 
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        Task PlayAsync(DiscreteSignal signal, int startPos = 0, int endPos = -1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        Task PlayAsync(string source, int startPos = 0, int endPos = -1);

        /// <summary>
        /// 
        /// </summary>
        void Pause();

        /// <summary>
        /// 
        /// </summary>
        void Resume();

        /// <summary>
        /// 
        /// </summary>
        void Stop();
    }
}
