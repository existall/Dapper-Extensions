using System.Collections.Generic;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public interface IPredicate
	{
		string GetSql(ISqlGenerationContext context, IDictionary<string, object> parameters);
	}

	public interface ISqlGenerationContext
	{
		ISqlDialect  Dialect { get; }
		IClassMapperRepository ClassMapperRepository { get; }
	}
}