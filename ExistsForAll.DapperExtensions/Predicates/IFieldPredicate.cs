namespace ExistsForAll.DapperExtensions.Predicates
{
	public interface IFieldPredicate : IComparePredicate
	{
		object Value { get; set; }
	}
}