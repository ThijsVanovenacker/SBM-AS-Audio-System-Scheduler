namespace Skyline.DataMiner.Library
{
	using System;
	using System.Linq;

	/// <summary>
	/// Class to Generate Random numbers.
	/// </summary>
	public static class RandomNumber
	{
		/// <summary>
		/// <see cref="System.Random"/> object used to generate the random numbers.
		/// </summary>
		private static readonly Random Random = new Random();

		/// <summary>
		/// Object used to sync.
		/// </summary>
		private static readonly object SyncLock = new object();

		/// <summary>
		/// Returns a random number within a specified range.
		/// </summary>
		/// <param name="min">The inclusive lower bound of the random number returned.</param>
		/// <param name="max">
		/// The exclusive upper bound of the random number returned. maxValue must be greater than or
		/// equal to minValue.
		/// </param>
		/// <returns>A double greater than or equal to minValue and less than maxValue.</returns>
		public static double GetRandomDouble(double min, double max)
		{
			lock (SyncLock)
			{
				return (Random.NextDouble() * (max - min)) + min;
			}
		}

		/// <summary>
		/// Returns a random number within a specified range.
		/// </summary>
		/// <param name="min">The inclusive lower bound of the random number returned.</param>
		/// <param name="max">
		/// The exclusive upper bound of the random number returned. maxValue must be greater than or
		/// equal to minValue.
		/// </param>
		/// <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue.</returns>
		public static int GetRandomNumber(int min, int max)
		{
			lock (SyncLock)
			{
				return Random.Next(min, max);
			}
		}

		/// <summary>
		/// Generates a random string with the provided number of characters.
		/// </summary>
		/// <param name="length"></param>
		/// <returns>A random string.</returns>
		public static string RandomString(int length)
		{
			if (length < 1)
			{
				throw new ArgumentOutOfRangeException("length", "length needs to be a positive number");
			}

			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
		}
	}
}