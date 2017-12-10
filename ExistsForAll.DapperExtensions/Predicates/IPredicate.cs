using System.Collections.Generic;

namespace ExistsForAll.DapperExtensions
{
	public interface IPredicate
	{
		string GetSql(ISqlGenerationContext context, IDictionary<string, object> parameters);
	}
}