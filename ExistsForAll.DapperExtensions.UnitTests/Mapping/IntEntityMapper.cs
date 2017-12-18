namespace ExistsForAll.DapperExtensions.UnitTests.Mapping
{
	public class IntEntityMapper : BaseMapper<IntEntity>
	{
		public IntEntityMapper()
		{
			Key(x => x.Id).GeneratedBy.Assigned();
		}
	}
}