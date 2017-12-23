using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.UnitTests.Mapping
{
	public class EnumEntityMapper : ClassMapper<EnumEntity>
	{
		public EnumEntityMapper()
		{
			Key(x => x.Id).GeneratedBy.Assigned();
			Map(x => x.String);
			Map(x => x.DateTime);
			Map(x => x.Guid);
			Map(x => x.Enum).CustomMapper(new EnumCustomType());
		}
	}
}