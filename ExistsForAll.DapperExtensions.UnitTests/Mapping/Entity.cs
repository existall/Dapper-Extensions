using System;

namespace ExistsForAll.DapperExtensions.UnitTests.Mapping
{
	public class Entity<T>
	{
		public T Id { get; set; }
		public string String { get; set; }
		public DateTime DateTime { get; set; }
		public Guid Guid { get; set; }
	}
}