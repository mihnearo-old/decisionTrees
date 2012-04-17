namespace mlLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class implements the ChiSquare value generator.
    /// </summary>
    public static class ChiSquare
    {
        /*  The following functions for calculating normal and
            chi-square probabilities and critical values were adapted by
            John Walker from C implementations
            written by Gary Perlman of Wang Institute, Tyngsboro, MA
            01879.  Both the original C code and this JavaScript edition
            are in the public domain.  */

        #region Constants

        private const double BIGX = 20.0;                  /* max value to represent exp(x) */
        private const double LOG_SQRT_PI = 0.5723649429247000870717135; /* log(sqrt(pi)) */
        private const double I_SQRT_PI = 0.5641895835477562869480795;   /* 1 / sqrt(pi) */

        #endregion

        /// <summary>
        /// probability of normal z value
        /// 
        /// Adapted from a polynomial approximation in:
        ///         Ibbetson D, Algorithm 209
        ///         Collected Algorithms of the CACM 1963 p. 616
        ///         
        /// Note:
        ///         This routine has six digit accuracy, so it is only useful for absolute
        ///         z values less than 6. For z values greater or equal to 6.0, poz() returns 0.0.
        /// </summary>
        /// <param name="z">value to approximate</param>
        /// <returns>probability value</returns>
        public static double ProbabilityOfNormalZValue(double z)
        {
            double y, x, w;
            var Z_MAX = 6.0;              /* Maximum meaningful z value */

            if (z == 0.0)
            {
                x = 0.0;
            }
            else
            {
                y = 0.5 * Math.Abs(z);
                if (y >= (Z_MAX * 0.5))
                {
                    x = 1.0;
                }
                else if (y < 1.0)
                {
                    w = y * y;
                    x = ((((((((0.000124818987 * w
                             - 0.001075204047) * w + 0.005198775019) * w
                             - 0.019198292004) * w + 0.059054035642) * w
                             - 0.151968751364) * w + 0.319152932694) * w
                             - 0.531923007300) * w + 0.797884560593) * y * 2.0;
                }
                else
                {
                    y -= 2.0;
                    x = (((((((((((((-0.000045255659 * y
                                   + 0.000152529290) * y - 0.000019538132) * y
                                   - 0.000676904986) * y + 0.001390604284) * y
                                   - 0.000794620820) * y - 0.002034254874) * y
                                   + 0.006549791214) * y - 0.010557625006) * y
                                   + 0.011630447319) * y - 0.009279453341) * y
                                   + 0.005353579108) * y - 0.002141268741) * y
                                   + 0.000535310849) * y + 0.999936657524;
                }
            }
            return z > 0.0 ? ((x + 1.0) * 0.5) : ((1.0 - x) * 0.5);
        }

        /// <summary>
        /// Compute exponential of a value with a max.
        /// </summary>
        /// <param name="x">value to compute</param>
        /// <returns>new value</returns>
        public static double Ex(double x)
        {
            return (x < -BIGX) ? 0.0 : Math.Exp(x);
        }

        /// <summary>
        /// Probability of chi-square value
        /// 
        /// Adapted from:
        ///               Hill, I. D. and Pike, M. C.  Algorithm 299
        ///               Collected Algorithms for the CACM 1967 p. 243
        ///       Updated for rounding errors based on remark in
        ///               ACM TOMS June 1985, page 185
        /// </summary>
        /// <param name="x">value to approximate</param>
        /// <param name="df">degress of freedom</param>
        /// <returns>probability value</returns>
        public static double ProbabilityOfChiSquareValue(double x, int df)
        {
            double a, y = 0.0, s;
            double e, c, z;
            bool even;                     /* True if df is an even number */

            if (x <= 0.0 || df < 1)
            {
                return 1.0;
            }

            a = 0.5 * x;
            even = (df & 1) == 0;
            if (df > 1)
            {
                y = Ex(-a);
            }
            s = (even ? y : (2.0 * ProbabilityOfNormalZValue(-Math.Sqrt(x))));
            if (df > 2)
            {
                x = 0.5 * (df - 1.0);
                z = (even ? 1.0 : 0.5);
                if (a > BIGX)
                {
                    e = (even ? 0.0 : LOG_SQRT_PI);
                    c = Math.Log(a);
                    while (z <= x)
                    {
                        e = Math.Log10(z) + e;
                        s += Ex(c * z - a - e);
                        z += 1.0;
                    }
                    return s;
                }
                else
                {
                    e = (even ? 1.0 : (I_SQRT_PI / Math.Sqrt(a)));
                    c = 0.0;
                    while (z <= x)
                    {
                        e = e * (a / z);
                        c = c + e;
                        z += 1.0;
                    }
                    return c * y + s;
                }
            }
            else
            {
                return s;
            }
        }

        /*  CRITCHI  --  Compute critical chi-square value to
                         produce given p.  We just do a bisection
                         search for a value within CHI_EPSILON,
                         relying on the monotonicity of pochisq().  */

        /// <summary>
        /// Compute critical chi-square value to
        /// produce given p.  We just do a bisection
        /// search for a value within CHI_EPSILON,
        /// relying on the monotonicity of pochisq().
        /// </summary>
        /// <param name="p">p-value</param>
        /// <param name="df">degrees of freedom</param>
        /// <returns>critical chi square threshold</returns>
        public static double CriticalChiSquareValue(double p, int df)
        {
            var CHI_EPSILON = 0.000001;   /* Accuracy of critchi approximation */
            var CHI_MAX = 99999.0;        /* Maximum chi-square value */
            var minchisq = 0.0;
            var maxchisq = CHI_MAX;
            double chisqval;

            if (p <= 0.0)
            {
                return maxchisq;
            }
            else if (p >= 1.0)
            {
                return 0.0;
            }

            // check the cache
            lock (ValueCache)
            {
                Dictionary<int, double> pValueList = null;
                if (ValueCache.TryGetValue(p, out pValueList)
                    && pValueList.TryGetValue(df, out chisqval))
                {
                    return chisqval;
                }
            }

            // compute value
            chisqval = df / Math.Sqrt(p);    /* fair first value */
            while ((maxchisq - minchisq) > CHI_EPSILON)
            {
                if (ProbabilityOfChiSquareValue(chisqval, df) < p)
                {
                    maxchisq = chisqval;
                }
                else
                {
                    minchisq = chisqval;
                }
                chisqval = (maxchisq + minchisq) * 0.5;
            }

            // add the value to the cache
            lock (ValueCache)
            {
                if (!ValueCache.ContainsKey(p))
                {
                    ValueCache.Add(p, new Dictionary<int, double>());
                }

                if (!ValueCache[p].ContainsKey(df))
                {
                    ValueCache[p].Add(df, chisqval);
                }
            }

            // return value
            return chisqval;
        }

        /// <summary>
        /// Critical Chi-Square values for the given probability at 1-300 degrees of freedom (0 indexed, so index 0 = 1 degree of freedom)
        /// </summary>
        private static Dictionary<double, Dictionary<int, double>> ValueCache = new Dictionary<double, Dictionary<int, double>>();
    }
}
