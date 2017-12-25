using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ExistsForAll.DapperExtensions.Predicates
{
	public static class Projections
	{
		public static IProjection Projection<T>(Expression<Func<T, object>> expression)
		{
			var propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
			return new Projection(propertyInfo.Name);
		}

		public static IProjectionSet Set<T>(Expression<Func<T, object>> expression, object value) where T : class
		{
			var propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
			return new ProjectionSet(propertyInfo.Name, value);
		}
	}
}