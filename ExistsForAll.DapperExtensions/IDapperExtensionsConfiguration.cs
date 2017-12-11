using System;
using System.Collections.Generic;
using System.Reflection;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public interface IDapperExtensionsConfiguration
	{
		Type DefaultMapper { get; }
		IList<Assembly> MappingAssemblies { get; }
		ISqlDialect Dialect { get; }
		IGuidCreator GuidCreator { get; }
		bool AutoPopulateKeyGuidValue { get; set; }
	}
}