using System.Collections.Generic;
using System.Reflection;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions.Postgresql
{
	public class PostgresqlDapperExtensionsBuilder : DapperExtensionsBuilderBase
	{
		public DapperInstances BuildImplementor(IEnumerable<Assembly> assemblies, DapperExtensionsConfiguration configuration)
		{
			// verify configuration;
			var classMapperRepository = this.GetInitializedRepository(assemblies, configuration);
			var sqlGenerator = new PostgresqlGenerator(configuration);

			configuration.Dialect = new PostgreSqlDialect();

			var dapperImplementor = new PostgresqlDapperImplementor(sqlGenerator, classMapperRepository, configuration);
			var dapperAsyncImplementor = new DapperAsyncImplementor(sqlGenerator, classMapperRepository, configuration);
			return new DapperInstances(dapperImplementor, dapperAsyncImplementor);
		}
	}
}