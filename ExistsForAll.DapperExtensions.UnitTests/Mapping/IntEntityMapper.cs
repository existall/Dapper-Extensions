namespace ExistsForAll.DapperExtensions.UnitTests.Mapping
{
	public class IntEntityMapper : BaseMappper<IntEntity>
	{
		public IntEntityMapper()
		{
			Key(x => x.Id).GeneratedBy.Assigned();
		}
	}
}