using System;
using System.Linq;
using System.Linq.Expressions;
using ExistsForAll.DapperExtensions.Mapper;
using Xunit;

namespace ExistsForAll.DapperExtensions.UnitTests.Mapping
{
	internal static class ClassMapperExtensions
	{
		public static bool MappingExists<T>(this ClassMapper<T> target, Expression<Func<T,object>> expression) where T : class
		{
			var propertyInfo = ReflectionHelper<T>.GetProperty(expression);

			return target.Properties.Count(x => x.Name == propertyInfo.Name) == 1;
		}

		public static bool KeyExists<T>(this ClassMapper<T> target, Expression<Func<T, object>> expression) where T : class
		{
			var propertyInfo = ReflectionHelper<T>.GetProperty(expression);

			return target.Keys.Count(x => x.Name == propertyInfo.Name) == 1;
		}
	}
}