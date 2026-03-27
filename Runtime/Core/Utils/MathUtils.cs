using FixedPointy;
using UnityEngine;

namespace Core.Utils
{

	public enum MathComparisons
	{
		Equal,
		GreaterThan,
		GreaterEqualThan,
		LesserThan,
		LesserEqualThan
	}

	public static class MathUtils
	{
		/// <summary>
		/// Comparison is from a -> b
		/// Ex: a: 4, b:2, isGreaterThan. a > b. This time will be true.
		/// </summary>
		public static bool MakeComparison(Fix a, Fix b, MathComparisons comparison)
		{
			switch (comparison)
			{
				case MathComparisons.Equal:
					return a == b;
				case MathComparisons.GreaterThan:
					return a > b;
				case MathComparisons.GreaterEqualThan:
					return a >= b;
				case MathComparisons.LesserThan:
					return a < b;
				case MathComparisons.LesserEqualThan:
					return a <= b;
				default:
					return false;
			}
		}

		// Functions are taken from https://easings.net/#easeInOutExpo
#region Ease functions
		public static float EaseOutExpo(float x)
		{
			return x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);
		}

		public static float EaseInCirc(float x)
		{
			return 1 - Mathf.Sqrt(1 - Mathf.Pow(x, 2));
		}


		public static float EaseInOutExpo(float x)
		{
			return x == 0
						? 0
						: x == 1
							? 1
							: x < 0.5f ? Mathf.Pow(2, 20 * x - 10) / 2
								: (2 - Mathf.Pow(2, -20 * x + 10)) / 2;
		}

		public static float Jump(float x)
		{
			// These are values found manually trying different slopes on the linear movement
			float value = (x - 0.2f) / 0.6f;
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