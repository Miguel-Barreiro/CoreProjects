using System.Text.RegularExpressions;

namespace Core.Utils
{
	public static class RegexUtils
	{
		public static bool Test(this string regex, string input, RegexOptions options = RegexOptions.None)
		{
			return Regex.IsMatch(input, regex, options);
		}

		public static string ReplaceRegex(this string input, string regex, string replacement)
		{
			return Regex.Replace(input, regex, replacement);
		}

		public static string ReplaceRegex(this string input, string regex, MatchEvaluator matchEvaluator)
		{
			return Regex.Replace(input, regex, matchEvaluator);
		}

		public static string[] QuickMatch(this string input, string regex)
		{
			MatchCollection matchCollection = Regex.Matches(input, regex);

			string[] matches = new string[matchCollection.Count];

			for (int i = 0; i < matchCollection.Count; i++)
			{
				matches[i] = matchCollection[i].Value;
			}

			return matches;
		}
	}
}