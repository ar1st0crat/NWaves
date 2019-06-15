using NWaves.Filters.Base;

namespace NWaves.Filters.Polyphase
{
    /// <summary>
    /// System of polyphase filters
    /// </summary>
    public class PolyphaseSystem
    {
        /// <summary>
        /// Polyphase filters
        /// </summary>
        public FirFilter[] Filters { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kernel"></param>
        /// <param name="filterCount"></param>
        /// <param name="type"></param>
        public PolyphaseSystem(double[] kernel, int filterCount, int type = 1)
        {
            Filters = new FirFilter[filterCount];

            var len = (kernel.Length + 1) / filterCount;

            for (var i = 0; i < Filters.Length; i++)
            {
                var filterKernel = new double[len];

                for (var j = 0; j < len; j++)
                {
                    var kernelPos = i + filterCount * j;

                    if (kernelPos < kernel.Length)
                    {
                        filterKernel[j] = kernel[kernelPos];
                    }
                }

                Filters[i] = new FirFilter(filterKernel);
            }

            // type-II -> reverse

            if (type == 2)
            {
                for (var i = 0; i < Filters.Length / 2; i++)
                {
                    var tmp = Filters[i];
                    Filters[i] = Filters[filterCount - 1 - i];
                    Filters[filterCount - 1 - i] = tmp;
                }
            }
        }
    }
}
