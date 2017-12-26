using System.Collections.Generic;
using System.Data;
using Dapper;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions.Postgresql
{
	public class PostgresqlDapperImplementor : DapperImplementor , IPostgresqlDapperImplementor
	{
		public PostgresqlDapperImplementor(ISqlGenerator sqlGenerator,
			IClassMapperRepository classMappers,
			IDapperExtensionsConfiguration dapperExtensionsConfiguration) 
			: base(sqlGenerator, classMappers, dapperExtensionsConfiguration)
		{
		}

		public void Upsert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var keyPredicate = classMap.GetKeyPredicate(entity);

			var parameters = new Dictionary<string, object>();
			var dynamicParameters = new DynamicParameters();
		}

	}
}
