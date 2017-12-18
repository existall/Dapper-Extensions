using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExistsForAll.DapperExtensions.Mapper
{
	public interface IClassMapperRepository
	{
		void Initialize(IEnumerable<Assembly> assemblies, Type mapperType);
		IClassMapper GetMap(Type entityType);
		IClassMapper GetMap<T>();
		void ClearCache();
	}
}