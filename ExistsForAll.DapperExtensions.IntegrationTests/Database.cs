using System;
using System.Collections.Generic;
using System.Data;
using ExistsForAll.DapperExtensions.Predicates;

namespace ExistsForAll.DapperExtensions.IntegrationTests
{
	public class Database : IDatabase
	{
		private readonly IDapperImplementor _dapper;

		private IDbTransaction _transaction;

		public Database(IDbConnection connection, IDapperImplementor dapperImplementor)
		{
			_dapper = dapperImplementor;
			Connection = connection;

			if (Connection.State != ConnectionState.Open)
			{
				Connection.Open();
			}
		}

		public bool HasActiveTransaction => _transaction != null;

		public IDbConnection Connection { get; }

		public void Dispose()
		{
			if (Connection.State != ConnectionState.Closed)
			{
				if (_transaction != null)
				{
					_transaction.Rollback();
				}

				Connection.Close();
			}
		}

		public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
		{
			_transaction = Connection.BeginTransaction(isolationLevel);
		}

		public void Commit()
		{
			_transaction.Commit();
			_transaction = null;
		}

		public void Rollback()
		{
			_transaction.Rollback();
			_transaction = null;
		}

		public void RunInTransaction(Action action)
		{
			BeginTransaction();
			try
			{
				action();
				Commit();
			}
			catch (Exception ex)
			{
				if (HasActiveTransaction)
				{
					Rollback();
				}

				throw ex;
			}
		}

		public T RunInTransaction<T>(Func<T> func)
		{
			BeginTransaction();
			try
			{
				var result = func();
				Commit();
				return result;
			}
			catch (Exception ex)
			{
				if (HasActiveTransaction)
				{
					Rollback();
				}

				throw ex;
			}
		}

		public T Get<T>(dynamic id, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			return (T)_dapper.Get<T>(Connection, id, transaction, commandTimeout);
		}

		public T Get<T>(dynamic id, int? commandTimeout) where T : class
		{
			return (T)_dapper.Get<T>(Connection, id, _transaction, commandTimeout);
		}

		public void Insert<T>(IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			_dapper.Insert(Connection, entities, transaction, commandTimeout);
		}

		public void Insert<T>(IEnumerable<T> entities, int? commandTimeout) where T : class
		{
			_dapper.Insert(Connection, entities, _transaction, commandTimeout);
		}

		public void Insert<T>(T entity, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			_dapper.Insert(Connection, entity, transaction, commandTimeout);
		}

		public void Insert<T>(T entity, int? commandTimeout) where T : class
		{
			_dapper.Insert(Connection, entity, _transaction, commandTimeout);
		}

		public bool Update<T>(T entity, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties) where T : class
		{
			return _dapper.Update(Connection, entity, transaction, commandTimeout);
		}

		public bool Update<T>(T entity, int? commandTimeout, bool ignoreAllKeyProperties) where T : class
		{
			return _dapper.Update(Connection, entity, _transaction, commandTimeout);
		}

		public bool Delete<T>(T entity, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			return _dapper.Delete(Connection, entity, transaction, commandTimeout);
		}

		public bool Delete<T>(T entity, int? commandTimeout) where T : class
		{
			return _dapper.Delete(Connection, entity, _transaction, commandTimeout);
		}

		public bool Delete<T>(object predicate, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			return _dapper.Delete<T>(Connection, predicate, transaction, commandTimeout);
		}

		public bool Delete<T>(object predicate, int? commandTimeout) where T : class
		{
			return _dapper.Delete<T>(Connection, predicate, _transaction, commandTimeout);
		}

		public IEnumerable<T> GetList<T>(object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class
		{
			return _dapper.GetList<T>(Connection, predicate, sort, transaction, commandTimeout, buffered, projections);
		}

		public IEnumerable<T> GetList<T>(object predicate, IList<ISort> sort, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class
		{
			return _dapper.GetList<T>(Connection, predicate, sort, _transaction, commandTimeout, buffered, projections);
		}

		public IEnumerable<T> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class
		{
			return _dapper.GetPage<T>(Connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, projections);
		}

		public IEnumerable<T> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class
		{
			return _dapper.GetPage<T>(Connection, predicate, sort, page, resultsPerPage, _transaction, commandTimeout, buffered, projections);
		}

		public IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class
		{
			return _dapper.GetSet<T>(Connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered, projections);
		}

		public IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class
		{
			return _dapper.GetSet<T>(Connection, predicate, sort, firstResult, maxResults, _transaction, commandTimeout, buffered, projections);
		}

		public int Count<T>(object predicate, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			return _dapper.Count<T>(Connection, predicate, transaction, commandTimeout);
		}

		public int Count<T>(object predicate, int? commandTimeout) where T : class
		{
			return _dapper.Count<T>(Connection, predicate, _transaction, commandTimeout);
		}

		public IMultipleResultReader GetMultiple(GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout)
		{
			return _dapper.GetMultiple(Connection, predicate, transaction, commandTimeout);
		}

		public IMultipleResultReader GetMultiple(GetMultiplePredicate predicate, int? commandTimeout)
		{
			return _dapper.GetMultiple(Connection, predicate, _transaction, commandTimeout);
		}

		public int AtomicIncrement<T>(object predicate, IProjection projection, int amount, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			return _dapper.AtomicIncrement<T>(Connection, predicate, projection, amount, transaction, commandTimeout);
		}

		public int AtomicIncrement<T>(object predicate, IProjection projection, int amount, int? commandTimeout) where T : class
		{
			return _dapper.AtomicIncrement<T>(Connection, predicate, projection, amount, _transaction, commandTimeout);
		}
	}
}