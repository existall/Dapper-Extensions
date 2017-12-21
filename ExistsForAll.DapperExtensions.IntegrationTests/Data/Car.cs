using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.IntegrationTests.Data
{
	class CarMapper : ClassMapper<Car>
	{
		public CarMapper()
		{
			Key(x => x.Id);
			Map(x => x.Name);
		}
	} 

    class Car
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
