using System;

namespace Core.Utils
{
	public static class UUID
	{
		private static Random Random = new(DateTimeOffset.Now.Millisecond);
		private static int CharacterCount = 11;

		/// <summary>
		/// Generate UUID which is returned as a string with a length based on CharacterCount.
		/// Implementation from https://stackoverflow.com/questions/1458468/youtube-like-guid
		/// </summary>
		public static string Generate()
		{
			int bitCount = 6 * CharacterCount;
			int byteCount = (int)Math.Ceiling(bitCount / 8f);
			byte[] buffer = new byte[byteCount];
			Random.NextBytes(buffer);

			string guid = Convert.ToBase64String(buffer);
			// Replace URL unfriendly characters
			guid = guid.Replace('+', '-').Replace('/', '_');
			// Trim characters to fit the count
			return guid.Substring(0, CharacterCount);
		}
	}

}