using System.Collections.Generic;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Predicates;

namespace ExistsForAll.DapperExtensions
{
	internal interface IGetGenerator
	{
		ActionParams GetList(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IList<IProjection> projections);

		ActionParams GetPage(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IList<IProjection> projections,
			int page, int resultsPerPage);

		ActionParams GetSet(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult,
			int maxResults, IList<IProjection> projections);
	}
}