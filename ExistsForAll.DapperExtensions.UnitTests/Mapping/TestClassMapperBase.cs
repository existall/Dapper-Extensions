using System;
using System.Linq.Expressions;
using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.UnitTests.Mapping
{
	public class TestClassMapperBase<T> : ClassMapper<T> where T : class
	{
		public new IdOptions Key<TOut>(Expression<Func<T, TOut>> expression)
		{
			return base.Key(expression);
		}

		public new IPropertyMap Map<TOut>(Expression<Func<T, TOut>> expression)
		{
			return base.Map(expression);
		}

		public new void UnMap<TOut>(Expression<Func<T, TOut>> expression)
		{
			base.UnMap(expression);
		}
	}
}