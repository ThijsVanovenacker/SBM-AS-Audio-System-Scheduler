namespace Skyline.DataMiner.Library
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	/// <summary>
	/// Class with <see cref="IEnumerable{T}"/> extension methods.
	/// </summary>
	[DebuggerStepThrough]
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Adds an item to an <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="T">Type of the IEnumerable items.</typeparam>
		/// <param name="sequence">Sequence to add the item.</param>
		/// <param name="item">Item to add.</param>
		/// <returns>The original sequence plus the item to add.</returns>
		public static IEnumerable<T> Add<T>(this IEnumerable<T> sequence, T item)
		{
			if (sequence == null)
			{
				throw new ArgumentNullException("sequence");
			}

			foreach (var i in sequence)
			{
				yield return i;
			}

			yield return item;
		}

		/// <summary>
		/// Performs an given action on every item of the sequence.
		/// </summary>
		/// <typeparam name="T">Type of the sequence.</typeparam>
		/// <param name="sequence">Sequence to perform the action.</param>
		/// <param name="action">Action to perform.</param>
		public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
		{
			if (sequence == null)
			{
				throw new ArgumentNullException("sequence");
			}

			if (action == null)
			{
				throw new ArgumentNullException("action");
			}

			foreach (var item in sequence)
			{
				action(item);
			}
		}

		/// <summary>
		/// Gets the first number in a string.
		/// </summary>
		/// <param name="input">Input string.</param>
		/// <returns>A new string with the first number.</returns>
		public static string GetFirstNumber(this IEnumerable<char> input)
		{
			return new string(input.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit).ToArray());
		}

		/// <summary>
		/// Gets a string until the first number.
		/// </summary>
		/// <param name="input">Input string.</param>
		/// <returns>A new string with the input string until the first number.</returns>
		public static string GetUntilFirstNumber(this IEnumerable<char> input)
		{
			return new string(input.TakeWhile(c => !char.IsDigit(c)).ToArray()) + input.GetFirstNumber();
		}

		/// <summary>
		/// Creates a <see cref="ConcurrentBag{T}"/> from an
		/// <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">
		/// An <see cref="IEnumerable{T}"/> to create a <see cref="ConcurrentBag{T}"/> from.
		/// </param>
		/// <returns>A <see cref="ConcurrentBag{T}"/> that contains values from the input sequence.</returns>
		public static ConcurrentBag<TSource> ToConcurrentBag<TSource>(this IEnumerable<TSource> source)
		{
			return new ConcurrentBag<TSource>(source);
		}

		/// <summary>
		/// Creates a <see cref="ConcurrentDictionary{TKey, TValue}"/> from an
		/// <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/>.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="source">
		/// An <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/>.
		/// </param>
		/// <returns>
		/// A <see cref="ConcurrentDictionary{TKey, TValue}"/> that contains values from the input sequence.
		/// </returns>
		public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
			this IEnumerable<KeyValuePair<TKey, TValue>> source)
		{
			return new ConcurrentDictionary<TKey, TValue>(source);
		}

		/// <summary>
		/// Creates a <see cref="ConcurrentDictionary{TKey, TValue}"/> from an
		/// <see cref="IEnumerable{T}"/> according to specified key selector and element
		/// selector functions.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
		/// <typeparam name="TValue">The type of the value returned by elementSelector.</typeparam>
		/// <param name="source">
		/// An <see cref="IEnumerable{T}"/> to create a <see cref="Dictionary{TKey, TValue}"/> from.
		/// </param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="valueSelector">
		/// A transform function to produce a result element value from each element.
		/// </param>
		/// <returns>
		/// A <see cref="ConcurrentDictionary{TKey, TValue}"/> that contains values of type TElement
		/// selected from the input sequence.
		/// </returns>
		public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TSource, TKey, TValue>(
			this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector,
			Func<TSource, TValue> valueSelector)
		{
			return
				new ConcurrentDictionary<TKey, TValue>(source.Select(x => new KeyValuePair<TKey, TValue>(keySelector(x), valueSelector(x))));
		}

		/// <summary>
		/// Creates a <see cref="Dictionary{TKey, TValue}"/> from an
		/// <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/>.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="source">
		/// An <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/>
		/// </param>
		/// <returns>
		/// A <see cref="Dictionary{TKey, TValue}"/> that contains values from the input sequence.
		/// </returns>
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
		{
			return source.ToDictionary(x => x.Key, x => x.Value);
		}

		/// <summary>
		/// Creates a <see cref="HashSet{T}"/> from an <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">
		/// An <see cref="IEnumerable{T}"/> to create a <see cref="HashSet{T}"/> from.
		/// </param>
		/// <returns>A <see cref="HashSet{T}"/> that contains values from the input sequence.</returns>
		public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source)
		{
			return new HashSet<TSource>(source);
		}

		/// <summary>
		/// Creates a <see cref="LinkedList{T}"/> from an <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">
		/// An <see cref="IEnumerable{T}"/> to create a <see cref="LinkedList{T}"/> from.
		/// </param>
		/// <returns>A <see cref="LinkedList{T}"/> that contains values from the input sequence.</returns>
		public static LinkedList<TSource> ToLinkedList<TSource>(this IEnumerable<TSource> source)
		{
			return new LinkedList<TSource>(source);
		}

		/// <summary>
		/// Creates a <see cref="Queue{T}"/> from an <see cref="IEnumerable{T}"/>
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">
		/// An <see cref="IEnumerable{T}"/> to create a <see cref="SortedSet{T}"/> from.
		/// </param>
		/// <returns>A <see cref="Queue{T}"/> that contains values from the input sequence.</returns>
		public static Queue<TSource> ToQueue<TSource>(this IEnumerable<TSource> source)
		{
			return new Queue<TSource>(source);
		}

		/// <summary>
		/// Creates a <see cref="SortedSet{T}"/> from an <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">
		/// An <see cref="IEnumerable{T}"/> to create a <see cref="SortedSet{T}"/> from.
		/// </param>
		/// <returns>A <see cref="SortedSet{T}"/> that contains values from the input sequence.</returns>
		public static SortedSet<TSource> ToSortedSet<TSource>(this IEnumerable<TSource> source)
		{
			return new SortedSet<TSource>(source);
		}

		/// <summary>
		/// Creates a <see cref="Stack{T}"/> from an <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">
		/// An <see cref="IEnumerable{T}"/> to create a <see cref="Stack{T}"/> from.
		/// </param>
		/// <returns>A <see cref="SortedSet{T}"/> that contains values from the input sequence.</returns>
		public static Stack<TSource> ToStack<TSource>(this IEnumerable<TSource> source)
		{
			return new Stack<TSource>(source);
		}
	}
}