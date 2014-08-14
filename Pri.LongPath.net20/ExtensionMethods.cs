using System;
using System.Collections.Generic;

namespace Pri.LongPath
{
	public static class ExtensionMethods
	{
		public static T[] ToArray<T>(this IEnumerable<T> source)
		{
			if (source == null) throw new ArgumentNullException("source");
			ICollection<T> collection = source as ICollection<T>;
			T[] array = null;
			if (collection != null)
			{
				array = new T[collection.Count];
				collection.CopyTo(array, 0);
				return array;
			}
			int length = 0;
			foreach (T element in source)
			{
				if (array == null)
					array = new T[4];
				else if (array.Length == length)
				{
					T[] elementArray = new T[checked(length * 2)];
					Array.Copy((Array)array, 0, (Array)elementArray, 0, length);
					array = elementArray;
				}
				array[length] = element;
				++length;
			}
			return array;
		}

		public static bool Contains<T>(this IEnumerable<T> source, T value)
		{
			if (source == null) throw new ArgumentNullException("source");
			ICollection<T> collection = source as ICollection<T>;
			if(collection != null) return collection.Contains(value);
			var comparer = EqualityComparer<T>.Default;
			foreach (T e in source)
			{
				if (comparer.Equals(e, value))
					return true;
			}
			return false;
		}

		public static IEnumerable<TResult> Select<TResult>(this IEnumerable<string> source, Func<string, TResult> selector)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (selector == null) throw new ArgumentNullException("selector");

			foreach (var e in source)
				yield return selector(e);
		}
	}

	public delegate TResult Func<in T, out TResult>(T arg);
}
