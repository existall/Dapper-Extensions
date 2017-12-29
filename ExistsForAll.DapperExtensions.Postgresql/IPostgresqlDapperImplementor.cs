using System.Data;
using System.Threading.Tasks;

namespace ExistsForAll.DapperExtensions.Postgresql
{
	public interface IPostgresqlDapperImplementor : IDapperImplementor
	{
		void Upsert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
		Task UpsertAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
	}
}