using System.Collections.Generic;

namespace ExistsForAll.DapperExtensions.Predicates
{
	public interface IPredicate
	{
		string GetSql(ISqlGenerationContext context, IDictionary<string, object> parameters);
	}
}