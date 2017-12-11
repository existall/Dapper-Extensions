using System;
using System.Collections.Generic;
using System.Reflection;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public class DapperExtensionsConfiguration : IDapperExtensionsConfiguration
	{
		public Type DefaultMapper { get; set; } = typeof(ClassMapper<>);
		public IList<Assembly> MappingAssemblies { get; } = new List<Assembly>();
		public ISqlDialect Dialect { get; set; } = new SqlServerDialect();
		public IGuidCreator GuidCreator { get; set; } = new GuidCreator();
		public bool AutoPopulateKeyGuidValue { get; set; }
	}
}