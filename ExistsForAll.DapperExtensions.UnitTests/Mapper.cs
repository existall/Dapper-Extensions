using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.UnitTests
{
	public class Mapper : ClassMapper<Foo>
	{
		public Mapper()
		{
			Key(x=>x.Name).GeneratedBy.Assigned();
		}
	}
}