namespace NWaves.Filters.Adaptive
{
    /// <summary>
    /// Represents Adaptive RLS filter (Recursive-Least-Squares algorithm).
    /// </summary>
    public class RlsFilter : AdaptiveFilter
    {
        /// <summary>
        /// Lambda.
        /// </summary>
        private readonly float _lambda;

        /// <summary>
        /// Inverse corr matrix.
        /// </summary>
        private readonly float[,] _p;

        /// <summary>
        /// Matrix of gain coefficients.
        /// </summary>
        private readonly float[] _gains;

        /// <summary>
        /// Temporary matrices for calculations.
        /// </summary>
        private readonly float[,] _dp, _tmp;

        /// <summary>
        /// Constructs <see cref="RlsFilter"/> of given <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="lambda">Lambda</param>
        /// <param name="initCorrMatrix">Value to initialize inverse corr matrix</param>
        public RlsFilter(int order, float lambda = 0.99f, float initCorrMatrix = 1e2f) : base(order)
        {
            _lambda = lambda;

            _p = new float[_kernelSize, _kernelSize];
            for (var i = 0; i < _kernelSize; i++)
            {
                _p[i, i] = initCorrMatrix;
            }

            _gains = new float[_kernelSize];
            _dp = new float[_kernelSize, _kernelSize];
            _tmp = new float[_kernelSize, _kernelSize];
        }

        /// <summary>
        /// Processes one sample of input and desired signals and adapts filter coefficients.
        /// </summary>
        /// <param name="input">Sample of input signal</param>
        /// <param name="desired">Sample of desired signal</param>
        public override float Process(float input, float desired)
        {
            var offset = _delayLineOffset;

            _delayLine[offset + _kernelSize] = input;   // duplicate it for better loop performance

            
            var y = Process(input);

            var e = desired - y;
                                 

            // ======================================================================
            // ============= lot of calculations before updating weights ============
            // ======================================================================

            // =========== calculate gain coefficients ===========
            // ===========   p*x / (lambda + xT*p*x)   ===========

            for (int i = 0; i < _kernelSize; _gains[i] = 0, i++) { }

            var g = _lambda;

            for (int i = 0; i < _kernelSize; i++)    // calculate  p*x
            {
                for (int k = 0, pos = offset; k < _kernelSize; k++, pos++)
                {
                    _gains[i] += _p[i, k] * _delayLine[pos];
                }
            }

            for (int k = 0, pos = offset; k < _kernelSize; k++, pos++)      // calculate xT*p*x
            {
                g += _gains[k] * _delayLine[pos];
            }

            for (int i = 0; i < _kernelSize; i++)
            {
                _gains[i] /= g;
            }

            // ============ update inv corr matrix ================
            // ========== (p - gain*xT*p) / lambda ================

            for (int i = 0; i < _kernelSize; i++)        // calculate  _tmp = gain * xT
            {
                for (int k = 0, pos = offset; k < _kernelSize; k++, pos++)
                {
                    _tmp[i, k] = _gains[i] * _delayLine[pos];
                }
            }

            for (int i = 0; i < _kernelSize; i++)        // calculate  _dp = _tmp * p
            {
                for (int j = 0; j < _kernelSize; j++)
                {
                    for (int k = 0; k < _kernelSize; k++)
                    {
                        _dp[i, j] = _tmp[i, k] * _p[k, j];
                    }
                }
            }

            for (int i = 0; i < _kernelSize; i++)        // update inv corr matrix
            {
                for (int j = 0; j < _kernelSize; j++)
                {
                    _p[i, j] = (_p[i, j] - _dp[i, j]) / _lambda;
                }
            }

            // ======================================================================
            // ===================== finally, update weights: =======================
            // ======================================================================

            for (int i = 0; i < _kernelSize; i++)
            {
                _b[i] = _b[_kernelSize + i] = _b[i] + _gains[i] * e;
            }

            return y;
        }
    }
}
