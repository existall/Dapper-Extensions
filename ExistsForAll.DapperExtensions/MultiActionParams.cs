using System.Collections.Generic;
using Dapper;

namespace ExistsForAll.DapperExtensions
{
	internal struct MultiActionParams
	{
		public string Sql { get; }
		public IList<DynamicParameters> DynamicParameterses { get; }

		public MultiActionParams(string sql, IList<DynamicParameters> dynamicParameterses)
		{
			Sql = sql;
			DynamicParameterses = dynamicParameterses;
		}

		public static MultiActionParams New(string sql, IList<DynamicParameters> dynamicParameterses)
		{
			return new MultiActionParams(sql, dynamicParameterses);
		}
	}
}