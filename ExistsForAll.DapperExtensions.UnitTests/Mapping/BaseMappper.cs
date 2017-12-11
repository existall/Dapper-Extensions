using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.UnitTests.Mapping
{
	public class BaseMappper<T> : ClassMapper<Entity<T>>
	{
		public BaseMappper()
		{
			Map(x => x.String);
			Map(x => x.DateTime);
			Map(x => x.Guid);
		}
	}
}