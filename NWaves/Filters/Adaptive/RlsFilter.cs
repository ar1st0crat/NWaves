namespace NWaves.Filters.Adaptive
{
    /// <summary>
    /// Adaptive filter (Recursive-Least-Squares algorithm)
    /// </summary>
    public class RlsFilter : AdaptiveFilter
    {
        /// <summary>
        /// Lambda
        /// </summary>
        private readonly float _lambda;

        /// <summary>
        /// Inverse corr matrix
        /// </summary>
        private float[,] _p;

        /// <summary>
        /// Matrix of gain coefficients
        /// </summary>
        private float[] _gains;

        /// <summary>
        /// Temporary matrices for calculations
        /// </summary>
        private float[,] _dp, _tmp;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="order"></param>
        /// <param name="mu"></param>
        /// <param name="weights"></param>
        /// <param name="leakage"></param>
        /// <param name="lambda"></param>
        /// <param name="initCoeff"></param>
        public RlsFilter(int order,
                         float mu = 0.1f,
                         float[] weights = null,
                         float lambda = 0.99f,
                         float initCoeff = 1e2f)
            : base(order, weights)
        {
            _lambda = lambda;

            _p = new float[_order, _order];
            for (var i = 0; i < _order; i++)
            {
                _p[i, i] = initCoeff;
            }

            _gains = new float[_order];
            _dp = new float[_order, _order];
            _tmp = new float[_order, _order];
        }

        /// <summary>
        /// Process input and desired samples
        /// </summary>
        /// <param name="input"></param>
        /// <param name="desired"></param>
        /// <returns></returns>
        public override float Process(float input, float desired)
        {
            // =========== calculate gain coefficients ===========
            // ===========   p*x / (lambda + xT*p*x)   ===========

            var g = _lambda;

            for (var i = 0; i < _order; i++)
            {
                _gains[i] = 0;
            }

            var pos = 0;

            for (var i = 0; i < _order; i++)    // calculate  p*x
            {
                pos = 0;
                for (var k = _delayLineOffset; k < _order; k++, pos++)
                {
                    _gains[i] += _p[i, pos] * _x[k];
                }
                for (var k = 0; k < _delayLineOffset; k++, pos++)
                {
                    _gains[i] += _p[i, pos] * _x[k];
                }
            }

            pos = 0;
            for (var k = _delayLineOffset; k < _order; k++, pos++)      // calculate xT*p*x
            {
                g += _x[k] * _gains[pos];
            }
            for (var k = 0; k < _delayLineOffset; k++, pos++)
            {
                g += _x[k] * _gains[pos];
            }

            for (var i = 0; i < _order; i++)
            {
                _gains[i] /= g;
            }

            // ============ update inv corr matrix ================
            // ========== (p - gain*xT*p) / lambda ================

            for (var i = 0; i < _order; i++)        // calculate  _tmp = gain * xT
            {
                pos = 0;
                for (var k = _delayLineOffset; k < _w.Length; k++, pos++)
                {
                    _tmp[i, pos] = _gains[i] * _x[k];
                }
                for (var k = 0; k < _delayLineOffset; k++, pos++)
                {
                    _tmp[i, pos] = _gains[i] * _x[k];
                }
            }

            for (var i = 0; i < _order; i++)        // calculate  _dp = _tmp * p
            {
                for (var j = 0; j < _order; j++)
                {
                    for (var k = 0; k < _order; k++)
                    {
                        _dp[i, j] = _tmp[i, k] * _p[k, j];
                    }
                }
            }

            for (var i = 0; i < _order; i++)        // update inv corr matrix
            {
                for (var j = 0; j < _order; j++)
                {
                    _p[i, j] = (_p[i, j] - _dp[i, j]) / _lambda;
                }
            }


            // ================= update weights ===================

            var y = Process(input);

            var e = desired - y;

            for (var i = 0; i < _order; i++)
            {
                _w[i] += _gains[i] * e;
            }

            return y;
        }
    }
}
