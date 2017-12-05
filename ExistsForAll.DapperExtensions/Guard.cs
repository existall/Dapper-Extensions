using System;
using System.Collections.Generic;
using System.Linq;

namespace ExistsForAll.DapperExtensions
{
	internal class Guard
	{
		public static void ArgumentNull<T>(T obj, string name) where T : class
		{ 
			if (obj == null)
				throw new ArgumentNullException(name);
		}

		public static void EnumerableArgumentNull<T>(IEnumerable<T> obj, string name) where T : class
		{
			var enumerable = obj as T[] ?? obj.ToArray();

			ArgumentNull(enumerable, name);

			if (!enumerable.Any())
				throw new ArgumentNullException(name, $"{name} can not be empty");
		}
	}
}
