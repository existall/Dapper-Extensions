using System.Collections;

namespace ExistsForAll.DapperExtensions.Predicates
{
	public interface IInPredicate : IPredicate
	{
		ICollection Collection { get; }
		bool Not { get; set; }
	}
}