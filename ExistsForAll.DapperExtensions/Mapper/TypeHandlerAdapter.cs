using System;
using System.Data;
using Dapper;

namespace ExistsForAll.DapperExtensions.Mapper
{
	internal class TypeHandlerAdapter : SqlMapper.ITypeHandler
	{
		private readonly ICustomsMapper _customsMapper;

		public TypeHandlerAdapter(ICustomsMapper customsMapper)
		{
			_customsMapper = customsMapper;
		}

		public void SetValue(IDbDataParameter parameter, object value)
		{
			parameter.Value = _customsMapper.IntoDatabase(value);
		}

		public object Parse(Type destinationType, object value)
		{
			return _customsMapper.FromDatabase(destinationType, value);
		}
	}
}