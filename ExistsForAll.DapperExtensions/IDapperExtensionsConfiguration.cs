using System;
using System.Collections.Generic;
using System.Reflection;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public interface IDapperExtensionsConfiguration
	{
		Type DefaultMapper { get; }
		IList<Assembly> MappingAssemblies { get; }
		ISqlDialect Dialect { get; }
		IClassMapper GetMap(Type entityType);
		IClassMapper GetMap<T>() where T : class;
		void ClearCache();
		Guid GetNextGuid();
	}
}