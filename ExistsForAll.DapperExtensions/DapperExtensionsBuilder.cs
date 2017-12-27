using System.Collections.Generic;
using System.Reflection;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public class DapperExtensionsBuilder : DapperExtensionsBuilderBase
	{
		public DapperInstances BuildImplementor(IEnumerable<Assembly> assemblies, IDapperExtensionsConfiguration configuration)
		{
			// verify configuration;

			var repository = GetInitializedRepository(assemblies, configuration);
			var sqlGenerator = new SqlGenerator(configuration);
			var dapperImplementor = new DapperImplementor(sqlGenerator, repository, configuration);
			var dapperAsyncImplementor = new DapperAsyncImplementor(sqlGenerator, repository, configuration);
			return new DapperInstances(dapperImplementor, dapperAsyncImplementor);
		}
	}
}
