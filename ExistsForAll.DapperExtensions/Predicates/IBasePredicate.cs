namespace ExistsForAll.DapperExtensions.Predicates
{
	public interface IBasePredicate : IPredicate
	{
		string PropertyName { get; set; }
	}
}