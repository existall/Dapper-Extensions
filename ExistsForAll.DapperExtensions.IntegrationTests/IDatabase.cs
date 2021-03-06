﻿using System;
using System.Collections.Generic;
using System.Data;
using ExistsForAll.DapperExtensions.Predicates;

namespace ExistsForAll.DapperExtensions.IntegrationTests
{
	public interface IDatabase : IDisposable
	{
		bool HasActiveTransaction { get; }
		IDbConnection Connection { get; }
		void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
		void Commit();
		void Rollback();
		void RunInTransaction(Action action);
		T RunInTransaction<T>(Func<T> func);
		T Get<T>(dynamic id, IDbTransaction transaction, int? commandTimeout = null) where T : class;
		T Get<T>(dynamic id, int? commandTimeout = null) where T : class;
		void Insert<T>(IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout = null) where T : class;
		void Insert<T>(IEnumerable<T> entities, int? commandTimeout = null) where T : class;
		void Insert<T>(T entity, IDbTransaction transaction, int? commandTimeout = null) where T : class;
		void Insert<T>(T entity, int? commandTimeout = null) where T : class;
		bool Update<T>(T entity, IDbTransaction transaction, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class;
		bool Update<T>(T entity, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class;

		bool Update<T>(IPredicate predicate, IList<IProjectionSet> projectionSets, int? commandTimeout,
			bool ignoreAllKeyProperties) where T : class;
		bool Delete<T>(T entity, IDbTransaction transaction, int? commandTimeout = null) where T : class;
		bool Delete<T>(T entity, int? commandTimeout = null) where T : class;
		bool Delete<T>(object predicate, IDbTransaction transaction, int? commandTimeout = null) where T : class;
		bool Delete<T>(object predicate, int? commandTimeout = null) where T : class;
		IEnumerable<T> GetList<T>(object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout = null, bool buffered = true, IList<IProjection> projections = null) where T : class;
		IEnumerable<T> GetList<T>(object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true, IList<IProjection> projections = null) where T : class;
		IEnumerable<T> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout = null, bool buffered = true, IList<IProjection> projections = null) where T : class;
		IEnumerable<T> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, int? commandTimeout = null, bool buffered = true, IList<IProjection> projections = null) where T : class;
		IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class;
		IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class;
		int Count<T>(object predicate, IDbTransaction transaction, int? commandTimeout = null) where T : class;
		int Count<T>(object predicate, int? commandTimeout = null) where T : class;
		IMultipleResultReader GetMultiple(GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout = null);
		IMultipleResultReader GetMultiple(GetMultiplePredicate predicate, int? commandTimeout = null);

		int AtomicIncrement<T>(object predicate, IProjection projection, int amount, IDbTransaction transaction,
			int? commandTimeout) where T : class;
		int AtomicIncrement<T>(object predicate, IProjection projection, int amount, int? commandTimeout) where T : class;
	}
}