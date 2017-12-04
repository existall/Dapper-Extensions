using System;
using System.Collections.Generic;
using System.Reflection;
using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions
{
	public interface IClassMapperRepository
	{
		void Initialize(IEnumerable<Assembly> assemblies, Type mapperType);
		IClassMapper GetMap(Type entityType);
		IClassMapper GetMap<T>() where T : class;
		void ClearCache();
	}
}