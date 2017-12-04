using System.Collections.Generic;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public interface IPredicate
	{
		string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters);
	}
}