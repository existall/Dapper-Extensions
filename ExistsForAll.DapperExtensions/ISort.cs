namespace ExistsForAll.DapperExtensions
{
	public interface ISort
	{
		string PropertyName { get; set; }
		bool Ascending { get; set; }
	}
}