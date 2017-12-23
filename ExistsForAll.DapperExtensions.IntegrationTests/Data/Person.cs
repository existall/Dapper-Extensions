using System;
using System.Collections.Generic;
using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.IntegrationTests.Data
{
	public class Person
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public DateTime DateCreated { get; set; }
		public bool Active { get; set; }
		public IEnumerable<Phone> Phones { get; private set; }
		public Sex Sex { get; set; }
	}

	public enum Sex
	{
		Male, 
		Female
	}

	public class Phone
	{
		public int Id { get; set; }
		public string Value { get; set; }
	}

	public class PersonMapper : ClassMapper<Person>
	{
		public PersonMapper()
		{
			Table("Person");

			Key(x => x.Id);
			Map(x => x.FirstName);
			Map(x => x.LastName);
			Map(x => x.DateCreated);
			Map(x => x.Active);
			Map(x => x.Sex).CustomMapper(new EnumCustomType());
			Map(m => m.Phones).Ignore();
		}
	}
}