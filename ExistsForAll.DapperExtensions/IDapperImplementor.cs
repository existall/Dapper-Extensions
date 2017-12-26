using System.Collections.Generic;
using System.Data;
using ExistsForAll.DapperExtensions.Predicates;

namespace ExistsForAll.DapperExtensions
{
	public interface IDapperImplementor
	{
		T Get<T>(IDbConnection connection, object id, IDbTransaction transaction, int? commandTimeout) where T : class;
		void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout) where T : class;
		void Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
		bool Update<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
		bool Update<T>(IDbConnection connection, IPredicate predicate, IList<IProjectionSet> projectionSets,IDbTransaction transaction, int? commandTimeout) where T : class;
		bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
		bool Delete<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class;
		IEnumerable<T> GetList<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class;
		IEnumerable<T> GetPage<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class;
		IEnumerable<T> GetSet<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class;
		int Count<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class;
		IMultipleResultReader GetMultiple(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout);

		int AtomicIncrement<T>(IDbConnection connection, object predicate, IProjection projection, int amount,
			IDbTransaction dbTransaction,
			int? commandTimeout) where T : class;
	}
}