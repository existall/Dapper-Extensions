using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.IntegrationTests.Data
{
	public class Multikey
	{
		public int Key1 { get; set; }
		public string Key2 { get; set; }
		public string Value { get; set; }
		//public DateTime Date { get; set; }
	}

	public class MultikeyMapper : ClassMapper<Multikey>
	{
		public MultikeyMapper()
		{
			Key(p => p.Key1).GeneratedBy.Assigned();
			Key(p => p.Key2).GeneratedBy.Assigned();
			//Map(p => p.Date).Ignore();
			Map(x => x.Value);
		}
	}
}