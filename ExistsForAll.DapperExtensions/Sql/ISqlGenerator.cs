using System.Collections.Generic;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Predicates;

namespace ExistsForAll.DapperExtensions.Sql
{
	public interface ISqlGenerator
	{
		string Select(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters, IList<IProjection> projection = null);
		string SelectPaged(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDictionary<string, object> parameters, IList<IProjection> projection = null);
		string SelectSet(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDictionary<string, object> parameters, IList<IProjection> projection = null);
		string Count(IClassMapper map, IPredicate predicate, IDictionary<string, object> parameters);

		string Insert(IClassMapper map);
		string Update(IClassMapper map, IPredicate predicate, IDictionary<string, object> parameters);
		string Update(IClassMapper classMap, IPredicate predicate, IList<IProjectionSet> projectionSets, Dictionary<string, object> parameters);
		string Delete(IClassMapper map, IPredicate predicate, IDictionary<string, object> parameters);

		string AtomicIncrement(IClassMapper map, IPredicate predicate, IDictionary<string, object> parameters, IProjection projection, int amount);

		string IdentitySql(IClassMapper map);
		bool SupportsMultipleStatements();
	}
}