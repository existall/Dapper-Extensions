namespace ExistsForAll.DapperExtensions
{
	public interface IBasePredicate : IPredicate
	{
		string PropertyName { get; set; }
	}
}