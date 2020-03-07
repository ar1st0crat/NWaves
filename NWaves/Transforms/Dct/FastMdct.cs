namespace NWaves.Transforms
{
    public class FastMdct : Mdct
    {
        public FastMdct(int dctSize) : base(dctSize, new FastDct4(dctSize))
        {
        }
    }
}
