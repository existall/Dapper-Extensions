using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ExistsForAll.DapperExtensions
{
	public static class ReflectionHelper
	{
		public static MemberInfo GetProperty<T, TOut>(Expression<Func<T, TOut>> lambda)
		{
			Expression expr = lambda;
			for (; ; )
			{
				switch (expr.NodeType)
				{
					case ExpressionType.Lambda:
						expr = ((LambdaExpression)expr).Body;
						break;
					case ExpressionType.Convert:
						expr = ((UnaryExpression)expr).Operand;
						break;
					case ExpressionType.MemberAccess:
						var memberExpression = (MemberExpression)expr;
						var mi = memberExpression.Member;
						return mi;
					default:
						return null;
				}
			}
		}

		public static IDictionary<string, object> GetObjectValues(object obj)
		{
			IDictionary<string, object> result = new Dictionary<string, object>();
			if (obj == null)
			{
				return result;
			}


			foreach (var propertyInfo in obj.GetType().GetProperties())
			{
				var name = propertyInfo.Name;
				var value = propertyInfo.GetValue(obj, null);
				result[name] = value;
			}

			return result;
		}

		private static readonly HashSet<Type> SimpleTypes = new HashSet<Type>
		{
			typeof(byte),
			typeof(sbyte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
			typeof(float),
			typeof(double),
			typeof(decimal),
			typeof(bool),
			typeof(string),
			typeof(char),
			typeof(Guid),
			typeof(DateTime),
			typeof(DateTimeOffset),
			typeof(byte[])
		};

		public static bool IsSimpleType(this Type type)
		{
			var actualType = type;
			if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				actualType = type.GetGenericArguments()[0];
			}

			return SimpleTypes.Contains(actualType);
		}

		public static string GetParameterName(this IDictionary<string, object> parameters, string parameterName, char parameterPrefix)
		{
			return $"{parameterPrefix}{parameterName}_{parameters.Count}";
		}

		public static string SetParameterName(this IDictionary<string, object> parameters, string parameterName, object value, char parameterPrefix)
		{
			string name = parameters.GetParameterName(parameterName, parameterPrefix);
			parameters.Add(name, value);
			return name;
		}

		public static string AppendStrings(this IEnumerable<string> list, string separator = ", ")
		{
			return string.Join(separator, list);
		}
	}
}