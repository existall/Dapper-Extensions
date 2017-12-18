using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.UnitTests.Mapping
{
	public class BaseMapper<T> : ClassMapper<Entity<T>>
	{
		public BaseMapper()
		{
			Map(x => x.String);
			Map(x => x.DateTime);
			Map(x => x.Guid);
		}
	}
}