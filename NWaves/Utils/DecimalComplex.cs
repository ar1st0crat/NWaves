namespace NWaves.Utils
{
    struct DecimalComplex
    {
        public decimal Real { get; }
        public decimal Imaginary { get; }

        public DecimalComplex(decimal real, decimal imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        public static DecimalComplex One => new DecimalComplex(1, 0);

        public static DecimalComplex operator +(DecimalComplex x, DecimalComplex y)
        {
            return new DecimalComplex(x.Real + y.Real, x.Imaginary + y.Imaginary);
        }

        public static DecimalComplex operator +(DecimalComplex x, decimal y)
        {
            return new DecimalComplex(x.Real + y, x.Imaginary);
        }

        public static DecimalComplex operator -(DecimalComplex x, DecimalComplex y)
        {
            return new DecimalComplex(x.Real - y.Real, x.Imaginary - y.Imaginary);
        }

        public static DecimalComplex operator *(DecimalComplex x, DecimalComplex y)
        {
            return new DecimalComplex(x.Real * y.Real - x.Imaginary * y.Imaginary,
                                      x.Real * y.Imaginary + x.Imaginary * y.Real);
        }

        public static DecimalComplex operator /(DecimalComplex x, DecimalComplex y)
        {
            var den = y.Real * y.Real + y.Imaginary * y.Imaginary;

            return new DecimalComplex((x.Real * y.Real + x.Imaginary * y.Imaginary) / den,
                                      (x.Imaginary * y.Real - x.Real * y.Imaginary) / den);
        }
    }
}