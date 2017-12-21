using System;
using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.IntegrationTests.Data
{
    public class Animal
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

	public class AnimalMapper : ClassMapper<Animal>
	{
		public AnimalMapper()
		{
			Key(x => x.Id).GeneratedBy.Assigned();
			Map(x => x.Name);
		}
	}
}
