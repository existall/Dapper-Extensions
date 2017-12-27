using System.Collections.Generic;
using System.Reflection;
using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions
{
	public abstract class DapperExtensionsBuilderBase
	{
		protected IClassMapperRepository GetInitializedRepository(IEnumerable<Assembly> assemblies, IDapperExtensionsConfiguration configuration)
		{
			var repository = new ClassMapperRepository();
			repository.Initialize(assemblies, configuration.DefaultMapper);
			return repository;
		}
	}
}