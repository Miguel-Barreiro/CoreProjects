using System.Collections.Generic;
using FixedPointy;
using UnityEngine;

namespace Core.Utils
{
	public static class FixedPointyUtils
    {
        public static Vector3 ToVector3(this FixVec2 fixVec2)
        {
            return new Vector3((float)fixVec2.X, 0f, (float)fixVec2.Y);
        }

        public static Vector2 ToVector2(this FixVec2 fixVec2)
        {
            return new Vector2((float)fixVec2.X, (float)fixVec2.Y);
        }

        public static Fix ToFix(float value)
        {
            Fraction fraction = Fraction.RealToFraction(value);
            return Fix.Ratio(fraction.N, fraction.D);
        }

        public static FixVec2 ToFixVec2(Vector3 vec)
        {
            return new FixVec2(ToFix(vec.x), ToFix(vec.z));
        }

        public static Fix LerpTo(this Fix lerpFrom, Fix lerpTo, Fix percent)
        {
            Fix diff = lerpTo - lerpFrom;
            return lerpFrom + (diff * percent);
        }

        public static FixVec2 LerpTo(this FixVec2 lerpFrom, FixVec2 lerpTo, Fix percent)
        {
            FixVec2 diff = lerpTo - lerpFrom;
            return lerpFrom + (diff * percent);
        }

        public static Fix RadiansToDegrees(Fix radians)
        {
            return radians * (Fix.Ratio(180, 1) / FixMath.PI);
        }

        public static Fix DegreesToRadians(Fix degrees)
        {
            return degrees * (FixMath.PI / Fix.Ratio(180, 1));
        }

        public static Fix Sum(IEnumerable<Fix> values)
        {
            Fix sum = Fix.Zero;

            foreach (Fix value in values)
            {
                sum += value;
            }

            return sum;
        }

        public static Fix Clamp(Fix value, Fix min, Fix max)
        {
            return FixMath.Min(FixMath.Max(value, min), max);
        }

        public static Fix SafeDivide(Fix a, Fix b)
        {
            return (a == 0 || b == 0) ? Fix.Zero : a / b;
        }

        // Functions are taken from https://easings.net/#easeInOutExpo
        #region Ease functions
        public static Fix EaseOutExpo(Fix x){
            return x == 1 ? 1 : 1 - FixMath.Pow(2, -10 * x);
        }

        public static Fix EaseInOutExpo(Fix x)
        {
            return x == 0
            ? 0
            : x == 1
            ? 1
            : x < Fix.Ratio(1,2) ? FixMath.Pow(2, 20 * x - 10) / 2
            : (2 - FixMath.Pow(2, -20 * x + 10)) / 2;
        }

        public static Fix Jump(Fix x)
        {
            // These are values found manually trying different slopes on the linear movement
            Fix value = (x - Fix.Ratio(1, 5)) / (Fix.Ratio(1, 10) + Fix.Ratio(1, 2));
            return 
            value < 0
                ? 0
                : value > 1
                ? 1
                : value;
        }
        #endregion
    }
}