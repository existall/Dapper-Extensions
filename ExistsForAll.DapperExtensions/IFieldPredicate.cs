namespace ExistsForAll.DapperExtensions
{
	public interface IFieldPredicate : IComparePredicate
	{
		object Value { get; set; }
	}
}