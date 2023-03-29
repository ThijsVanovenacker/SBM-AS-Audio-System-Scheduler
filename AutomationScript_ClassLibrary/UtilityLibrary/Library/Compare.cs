namespace Skyline.DataMiner.Library
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	/// <summary>
	/// Class to implement DistinctBy extension method.
	/// </summary>
	[DebuggerStepThrough]
	public static class Compare
	{
		/// <summary>
		/// Returns distinct elements from a sequence by using the specified clause.
		/// </summary>
		/// <typeparam name="T">Type of the source IEnumerable.</typeparam>
		/// <typeparam name="TIdentity">Type of the selector object.</typeparam>
		/// <param name="source">IEnumerable object with the source sequence.</param>
		/// <param name="identitySelector">Function with the desired clause.</param>
		/// <returns>The distinct elements from a sequence by using the specified clause.</returns>
		public static IEnumerable<T> DistinctBy<T, TIdentity>(this IEnumerable<T> source, Func<T, TIdentity> identitySelector)
		{
			return source.Distinct(By(identitySelector));
		}

		/// <summary>
		/// Produces the set intersection of two sequences by using <paramref name="identitySelector"/>.
		/// </summary>
		/// <typeparam name="T">The type of the elements of the input sequences.</typeparam>
		/// <typeparam name="TIdentity">Type of the selector object.</typeparam>
		/// <param name="first">An <see cref="IEnumerable{T}"/> whose distinct elements that also appear in second will be returned.</param>
		/// <param name="second">An <see cref="IEnumerable{T}"/> whose distinct elements that also appear in the first sequence will be returned.</param>
		/// <param name="identitySelector">Function with the desired clause.</param>
		/// <returns>A sequence that contains the elements that form the set intersection of two sequences.</returns>
		public static IEnumerable<T> IntersectBy<T, TIdentity>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, TIdentity> identitySelector)
		{
			return first.Intersect(second, By(identitySelector));
		}

		/// <summary>
		/// Selects the desired IEqualityComparer.
		/// </summary>
		/// <typeparam name="TSource">Type of the Source.</typeparam>
		/// <typeparam name="TIdentity">Type of the identity to compare.</typeparam>
		/// <param name="identitySelector">Func with the desired compare selector.</param>
		/// <returns>The desired IEqualityComparer.</returns>
		internal static IEqualityComparer<TSource> By<TSource, TIdentity>(Func<TSource, TIdentity> identitySelector)
		{
			return new DelegateComparer<TSource, TIdentity>(identitySelector);
		}

		private class DelegateComparer<T, TIdentity> : IEqualityComparer<T>
		{
			private readonly Func<T, TIdentity> identitySelector;

			public DelegateComparer(Func<T, TIdentity> identitySelector)
			{
				this.identitySelector = identitySelector;
			}

			public bool Equals(T x, T y)
			{
				return Equals(this.identitySelector(x), this.identitySelector(y));
			}

			public int GetHashCode(T obj)
			{
				return this.identitySelector(obj).GetHashCode();
			}
		}
	}
}