using System;
using System.Linq;
using System.Linq.Expressions;
using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.UnitTests.Mapping
{
	internal static class ClassMapperExtensions
	{
		public static bool MappingExists<T>(this ClassMapper<T> target, Expression<Func<T,object>> expression) where T : class
		{
			var propertyInfo = ReflectionHelper.GetProperty(expression);

			return target.Properties.Names.Contains(propertyInfo.Name);
		}

		public static bool KeyExists<T>(this ClassMapper<T> target, Expression<Func<T, object>> expression) where T : class
		{
			var propertyInfo = ReflectionHelper.GetProperty(expression);

			return target.Keys.Names.Contains(propertyInfo.Name);
		}
	}
}