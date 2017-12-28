using Dapper;

namespace ExistsForAll.DapperExtensions
{
	internal struct ActionParams
	{
		private ActionParams(string sql, DynamicParameters dynamicParameterses)
		{
			Sql = sql;
			DynamicParameterses = dynamicParameterses;
		}

		public string Sql { get; }
		public DynamicParameters DynamicParameterses { get; }

		public static ActionParams New(string sql, DynamicParameters dynamicParameterses)
		{
			return new ActionParams(sql, dynamicParameterses);
		} 
	}
}