using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ExistsForAll.DapperExtensions
{
	public static class ReflectionHelper<T>
	{
		public static MemberInfo GetProperty<TOut>(Expression<Func<T, TOut>> lambda)
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
	}
}