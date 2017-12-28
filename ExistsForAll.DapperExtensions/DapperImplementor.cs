﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Dapper;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Predicates;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public class DapperImplementor : IDapperImplementor
	{
		protected IClassMapperRepository ClassMappers { get; }
		protected IDapperExtensionsConfiguration Configuration { get; }
		protected ISqlGenerator SqlGenerator { get; }

		private readonly IGetGenerator _getGenerator;
		private readonly IInsertGenerator _insertGenerator;
		private readonly IUpdateActionsProvider _updateActionsProvider;

		public DapperImplementor(ISqlGenerator sqlGenerator,
			IClassMapperRepository classMappers,
			IDapperExtensionsConfiguration dapperExtensionsConfiguration)
		{
			ClassMappers = classMappers;
			Configuration = dapperExtensionsConfiguration;
			SqlGenerator = sqlGenerator;

			_getGenerator = new GetGenerator(Configuration, classMappers, SqlGenerator);
			_insertGenerator = new InsertGenerator(Configuration, classMappers, SqlGenerator);
			_updateActionsProvider = new UpdateActionsProvider(dapperExtensionsConfiguration, ClassMappers, SqlGenerator);
		}

		public T Get<T>(IDbConnection connection, object id, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var predicate = classMap.GetIdPredicate(id);
			var result = GetList<T>(connection, classMap, predicate, null, transaction, commandTimeout, true, null)
				.SingleOrDefault();
			return result;
		}

		public void Insert<T>(IDbConnection connection,
			IEnumerable<T> entities,
			IDbTransaction transaction,
			int? commandTimeout) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();

			var actionParams = _insertGenerator.InsertBatch(classMap, entities);

			connection.Execute(actionParams.Sql, actionParams.DynamicParameterses, transaction, commandTimeout, CommandType.Text);
		}

		public void Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout)
			where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var autoGeneratedProperty = classMap.GetAutoGeneratedId();

			var actionParams = _insertGenerator.Insert(classMap, entity);

			if (autoGeneratedProperty != null)
			{
				IEnumerable<long> result;
				if (SqlGenerator.SupportsMultipleStatements())
				{
					result = connection.Query<long>(actionParams.Sql, actionParams.DynamicParameterses, transaction, false, commandTimeout, CommandType.Text);
				}
				else
				{
					connection.Execute(actionParams.Sql, actionParams.DynamicParameterses, transaction, commandTimeout, CommandType.Text);
					var sql = SqlGenerator.IdentitySql(classMap);
					result = connection.Query<long>(sql, actionParams.DynamicParameterses, transaction, false, commandTimeout, CommandType.Text);
				}

				var hasResult = false;
				object identityInt = null;
				foreach (var identityValue in result)
				{
					if (hasResult)
					{
						continue;
					}

					identityInt = identityValue;
					hasResult = true;
				}
				if (!hasResult)
				{
					throw new InvalidOperationException("The source sequence is empty.");
				}

				autoGeneratedProperty.Setter(entity, Convert.ChangeType(identityInt, autoGeneratedProperty.PropertyInfo.PropertyType));
				return;
			}

			connection.Execute(actionParams.Sql, actionParams.DynamicParameterses, transaction, commandTimeout, CommandType.Text);
		}

		public bool Update<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout)
			where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var actionParams = _updateActionsProvider.Update(classMap, entity);

			return connection.Execute(actionParams.Sql, actionParams.DynamicParameterses, transaction, commandTimeout, CommandType.Text) > 0;
		}

		public bool Update<T>(IDbConnection connection,
			IPredicate predicate,
			IList<IProjectionSet> projectionSets,
			IDbTransaction transaction,
			int? commandTimeout) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();

			var actionParams = _updateActionsProvider.Update(classMap, predicate, projectionSets);

			return connection.Execute(actionParams.Sql, actionParams.DynamicParameterses, transaction, commandTimeout, CommandType.Text) > 0;
		}

		public bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout)
			where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var predicate = classMap.GetKeyPredicate(entity);
			return Delete<T>(connection, classMap, predicate, transaction, commandTimeout);
		}

		public bool Delete<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout)
			where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var wherePredicate = classMap.GetPredicate(predicate);
			return Delete<T>(connection, classMap, wherePredicate, transaction, commandTimeout);
		}

		public IEnumerable<T> GetList<T>(
			IDbConnection connection,
			object predicate,
			IList<ISort> sort,
			IDbTransaction transaction,
			int? commandTimeout,
			bool buffered,
			IList<IProjection> projections = null) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var wherePredicate = classMap.GetPredicate(predicate);
			return GetList<T>(connection, classMap, wherePredicate, sort, transaction, commandTimeout, buffered, projections);
		}

		public IEnumerable<T> GetPage<T>(
			IDbConnection connection,
			object predicate,
			IList<ISort> sort,
			int page,
			int resultsPerPage,
			IDbTransaction transaction,
			int? commandTimeout,
			bool buffered,
			IList<IProjection> projections = null) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var wherePredicate = classMap.GetPredicate(predicate);
			return GetPage<T>(connection, classMap, wherePredicate, sort, page, resultsPerPage, transaction, commandTimeout,
				buffered, projections);
		}

		public IEnumerable<T> GetSet<T>(
			IDbConnection connection,
			object predicate,
			IList<ISort> sort,
			int firstResult,
			int maxResults,
			IDbTransaction transaction,
			int? commandTimeout,
			bool buffered,
			IList<IProjection> projections = null) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var wherePredicate = classMap.GetPredicate(predicate);
			return GetSet<T>(connection, classMap, wherePredicate, sort, firstResult, maxResults, transaction, commandTimeout,
				buffered, projections);
		}

		public int Count<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout)
			where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var wherePredicate = classMap.GetPredicate(predicate);
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Count(classMap, wherePredicate, parameters);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return (int)connection.Query(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text).Single()
				.Total;
		}

		public IMultipleResultReader GetMultiple(IDbConnection connection, GetMultiplePredicate predicate,
			IDbTransaction transaction, int? commandTimeout)
		{
			if (SqlGenerator.SupportsMultipleStatements())
			{
				return GetMultipleByBatch(connection, predicate, transaction, commandTimeout);
			}

			return GetMultipleBySequence(connection, predicate, transaction, commandTimeout);
		}

		public int AtomicIncrement<T>(IDbConnection connection,
			object predicate,
			IProjection projection,
			int amount,
			IDbTransaction dbTransaction,
			int? commandTimeout) where T : class
		{
			Guard.ArgumentNull(projection, nameof(projection));
			Guard.ArgumentNull(predicate, nameof(predicate));

			var classMap = ClassMappers.GetMap<T>();
			var target = classMap.GetPropertyMapByName(projection.PropertyName);

			if (target.Ignored || target.IsReadOnly)
				throw new InvalidOperationException(
					$"Atomic increment is not allowed on {projection.PropertyName} for type {classMap.EntityType}. It's either ignored or read only");

			var wherePredicate = classMap.GetPredicate(predicate);
			var parameters = new Dictionary<string, object>();

			var sql = SqlGenerator.AtomicIncrement(classMap, wherePredicate, parameters, projection, amount);

			return connection.Execute(sql, parameters, dbTransaction, commandTimeout, CommandType.Text);
		}

		protected IEnumerable<T> GetList<T>(IDbConnection connection,
			IClassMapper classMap,
			IPredicate predicate,
			IList<ISort> sort,
			IDbTransaction transaction,
			int? commandTimeout,
			bool buffered,
			IList<IProjection> projections) where T : class
		{
			var actionParams = _getGenerator.GetList(classMap, predicate, sort, projections);

			return connection.Query<T>(actionParams.Sql, actionParams.DynamicParameterses, transaction, buffered, commandTimeout, CommandType.Text);
		}

		protected IEnumerable<T> GetPage<T>(IDbConnection connection,
			IClassMapper classMap,
			IPredicate predicate,
			IList<ISort> sort,
			int page,
			int resultsPerPage,
			IDbTransaction transaction,
			int? commandTimeout,
			bool buffered,
			IList<IProjection> projections)
			where T : class
		{
			var actionParams = _getGenerator.GetPage(classMap, predicate, sort, projections, page, resultsPerPage);

			return connection.Query<T>(actionParams.Sql, actionParams.DynamicParameterses, transaction, buffered, commandTimeout, CommandType.Text);
		}

		protected IEnumerable<T> GetSet<T>(IDbConnection connection,
			IClassMapper classMap,
			IPredicate predicate,
			IList<ISort> sort,
			int firstResult,
			int maxResults,
			IDbTransaction transaction,
			int? commandTimeout,
			bool buffered,
			IList<IProjection> projections)
			where T : class
		{
			var actionParams = _getGenerator.GetSet(classMap, predicate, sort, firstResult, maxResults, projections);

			return connection.Query<T>(actionParams.Sql, actionParams.DynamicParameterses, transaction, buffered, commandTimeout, CommandType.Text);
		}

		protected bool Delete<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate,
			IDbTransaction transaction, int? commandTimeout) where T : class
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Delete(classMap, predicate, parameters);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
		}

		protected GridReaderResultReader GetMultipleByBatch(IDbConnection connection, GetMultiplePredicate predicate,
			IDbTransaction transaction, int? commandTimeout)
		{
			var parameters = new Dictionary<string, object>();
			var sql = new StringBuilder();
			foreach (var item in predicate.Items)
			{
				var classMap = ClassMappers.GetMap(item.Type);
				var itemPredicate = item.Value as IPredicate;
				if (itemPredicate == null && item.Value != null)
				{
					itemPredicate = classMap.GetPredicate(item.Value);
				}

				sql.AppendLine(SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters) +
							   Configuration.Dialect.BatchSeparator);
			}

			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			var grid = connection.QueryMultiple(sql.ToString(), dynamicParameters, transaction, commandTimeout,
				CommandType.Text);
			return new GridReaderResultReader(grid);
		}

		protected SequenceReaderResultReader GetMultipleBySequence(IDbConnection connection, GetMultiplePredicate predicate,
			IDbTransaction transaction, int? commandTimeout)
		{
			IList<SqlMapper.GridReader> items = new List<SqlMapper.GridReader>();
			foreach (var item in predicate.Items)
			{
				var parameters = new Dictionary<string, object>();
				var classMap = ClassMappers.GetMap(item.Type);
				var itemPredicate = item.Value as IPredicate;
				if (itemPredicate == null && item.Value != null)
				{
					itemPredicate = classMap.GetPredicate(item.Value);
				}

				var sql = SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters);
				var dynamicParameters = new DynamicParameters();
				foreach (var parameter in parameters)
				{
					dynamicParameters.Add(parameter.Key, parameter.Value);
				}

				var queryResult = connection.QueryMultiple(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
				items.Add(queryResult);
			}

			return new SequenceReaderResultReader(items);
		}
	}
}