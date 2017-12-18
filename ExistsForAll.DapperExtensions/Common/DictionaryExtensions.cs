using System;
using System.Collections.Generic;
using System.Text;

namespace ExistsForAll.DapperExtensions.Common
{
	public static class DictionaryExtensions
	{
		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
			TKey key, Func<TValue> valueCreator)
		{
			if (dictionary.TryGetValue(key, out var value))
				return value;

			value = valueCreator();
			dictionary.Add(key, value);
			return value;
		}

		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
			TKey key) where TValue : new()
		{
			return dictionary.GetOrAdd(key, () => new TValue());
		}
	}
}
