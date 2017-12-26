using System.Data;

namespace ExistsForAll.DapperExtensions.Postgresql
{
	public interface IPostgresqlDapperImplementor : IDapperImplementor
	{
		void Upsert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
	}
}