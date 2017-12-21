using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ExistsForAll.DapperExtensions.Mapper
{
	internal static class PropertyMapBuilder
	{
		public static IPropertyMap BuildMap<T, TOut>(Expression<Func<T,TOut>> expression)
		{
			var propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
			var property = new PropertyMap<T, TOut>(propertyInfo, expression.Compile(), GenerateSetterMethod<T, TOut>(propertyInfo));
			return property;
		}

		public static Action<T, TOut> GenerateSetterMethod<T,TOut>(PropertyInfo propertyInfo)
		{
			var setType = typeof(Action<,>).MakeGenericType(typeof(T), propertyInfo.PropertyType);

			var setter = propertyInfo.GetSetMethod()?.CreateDelegate(setType);

			if (setter == null)
			{
				return (x, y) => throw new ClassMapException($"No Set method found for {typeof(T)}.{propertyInfo.Name}, check the ClassMap");
			}

			return setter as Action<T, TOut>;
		}
	}
}