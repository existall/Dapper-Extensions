using System.Collections.Generic;

namespace ExistsForAll.DapperExtensions
{
	public interface IPredicateGroup : IPredicate
	{
		GroupOperator Operator { get; set; }
		IList<IPredicate> Predicates { get; set; }
	}
}