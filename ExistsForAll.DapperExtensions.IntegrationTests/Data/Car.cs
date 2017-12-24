using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.IntegrationTests.Data
{
	class CarMapper : ClassMapper<Car>
	{
		public CarMapper()
		{
			Key(x => x.Id).GeneratedBy.Assigned();
			Map(x => x.Name);
			Map(x => x.Hand);
		}
	} 

    class Car
    {
        public string Id { get; set; }
        public string Name { get; set; }
	    public int Hand { get; set; }
    }
}
