using System.Collections.Generic;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Predicates;

namespace ExistsForAll.DapperExtensions.Sql
{
	public interface ISqlGenerator
	{
		string Select(IClassMapper map, IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters);
		string SelectPaged(IClassMapper map, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDictionary<string, object> parameters);
		string SelectSet(IClassMapper map, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDictionary<string, object> parameters);
		string Count(IClassMapper map, IPredicate predicate, IDictionary<string, object> parameters);

		string Insert(IClassMapper map);
		string Update(IClassMapper map, IPredicate predicate, IDictionary<string, object> parameters);
		string Delete(IClassMapper map, IPredicate predicate, IDictionary<string, object> parameters);

		string IdentitySql(IClassMapper map);
		bool SupportsMultipleStatements();
	}
}