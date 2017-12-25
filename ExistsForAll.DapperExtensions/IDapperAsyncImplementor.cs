using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ExistsForAll.DapperExtensions.Predicates;

namespace ExistsForAll.DapperExtensions
{
	/// <summary>
	/// 
	/// </summary>
	public interface IDapperAsyncImplementor
	{
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Get{T}"/>.
		/// </summary>
		Task<T> GetAsync<T>(IDbConnection connection, object id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetList{T}"/>.
		/// </summary>
		Task<IEnumerable<T>> GetListAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, IList<IProjection> projections = null) where T : class;
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetPage{T}"/>.
		/// </summary>
		Task<IEnumerable<T>> GetPageAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int page = 1, int resultsPerPage = 10, IDbTransaction transaction = null, int? commandTimeout = null, IList<IProjection> projections = null) where T : class;
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetSet{T}"/>.
		/// </summary>
		Task<IEnumerable<T>> GetSetAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, IDbTransaction transaction = null, int? commandTimeout = null, IList<IProjection> projections = null) where T : class;
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Count{T}"/>.
		/// </summary>
		Task<int> CountAsync<T>(IDbConnection connection, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Insert{T}(IDbConnection, IEnumerable{T}, IDbTransaction, int?)"/>.
		/// </summary>
		Task InsertAsync<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class;
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Insert{T}(IDbConnection, T, IDbTransaction, int?)"/>.
		/// </summary>
		Task InsertAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class;
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Update{T}(IDbConnection, T, IDbTransaction, int?)"/>.
		/// </summary>
		Task<bool> UpdateAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties = false) where T : class;
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Delete{T}(IDbConnection, T, IDbTransaction, int?)"/>.
		/// </summary>
		Task<bool> DeleteAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Delete{T}(IDbConnection, object, IDbTransaction, int?)"/>.
		/// </summary>
		Task<bool> DeleteAsync<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class;

		Task<int> AtomicIncrementAsync<T>(IDbConnection connection,
			object predicate,
			IProjection projection,
			int amount,
			IDbTransaction dbTransaction,
			int? commandTimeout) where T : class;
	}
}