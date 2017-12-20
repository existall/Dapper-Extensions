using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ExistsForAll.DapperExtensions.Mapper
{
	internal static class PropertyMapBuilder
	{
		public static IPropertyMap BuildMap<T, TOut>(Expression<Func<T,TOut>> expression)
		{
			var propertyInfo = ReflectionHelper<T>.GetProperty(expression) as PropertyInfo;
			var property = new PropertyMap<T, TOut>(propertyInfo, expression.Compile(), GenerateSetterMethod<T, TOut>(propertyInfo));
			return property;
		}

		public static Action<T, TOut> GenerateSetterMethod<T,TOut>(PropertyInfo propertyInfo)
		{
			var setType = typeof(Action<,>).MakeGenericType(new[] { typeof(T), propertyInfo.PropertyType });

			var @delegate = propertyInfo.GetSetMethod()?.CreateDelegate(setType);

			return @delegate as Action<T, TOut>;

			//var memberExpression = (MemberExpression)expression.Body;

			//var target = Expression.Parameter(typeof(T), "x");
			//var value = Expression.Parameter(memberExpression.Type, "v");

			//var memberProperty = Expression.MakeMemberAccess(target, memberExpression.Member);
			//var assignment = Expression.Assign(memberProperty, value);

			//var lambda = Expression.Lambda<Action<T, TOut>>(assignment, target, value);
			//return lambda.Compile();
		}
	}
}