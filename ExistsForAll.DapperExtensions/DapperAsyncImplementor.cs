﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Predicates;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public class DapperAsyncImplementor : IDapperAsyncImplementor
	{
		private ISqlGenerator SqlGenerator { get; }
		private IClassMapperRepository ClassMappers { get; }
		private IDapperExtensionsConfiguration Configuration { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DapperAsyncImplementor"/> class.
		/// </summary>
		/// <param name="sqlGenerator">The SQL generator.</param>
		public DapperAsyncImplementor(ISqlGenerator sqlGenerator,
			IClassMapperRepository classMappers,
			IDapperExtensionsConfiguration dapperExtensionsConfiguration)
		{
			SqlGenerator = sqlGenerator;
			ClassMappers = classMappers;
			Configuration = dapperExtensionsConfiguration;
		}

		#region Implementation of IDapperAsyncImplementor
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Insert{T}(IDbConnection, IEnumerable{T}, IDbTransaction, int?)"/>.
		/// </summary>
		public async Task InsertAsync<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();

			var parameters = new List<DynamicParameters>();

			var columns = classMap.GetMutableColumns();

			var autoGeneratedProperty = classMap.GetAutoGeneratedId();

			var guids = new IPropertyMap[0];

			if (Configuration.AutoPopulateKeyGuidValue)
			{
				guids = columns.Where(x => x.PropertyInfo.PropertyType == typeof(Guid)).ToArray();
			}

			foreach (var e in entities)
			{
				foreach (var guid in guids)
				{
					if ((Guid)guid.Getter(e) == Guid.Empty)
					{
						var value = Configuration.GuidCreator.GetGuid();
						guid.Setter(e, value);
					}
				}

				var dynamicParameters = new DynamicParameters();

				foreach (var column in columns)
				{
					dynamicParameters.Add(column.Name, column.Getter(e));
				}

				if (autoGeneratedProperty != null)
				{
					var defaultValue = autoGeneratedProperty.Getter(e);
					//dynamicParameters.Add("IdOutParam", direction: ParameterDirection.Output, value: defaultValue);
				}

				parameters.Add(dynamicParameters);
			}

			var sql = SqlGenerator.Insert(classMap);

			await connection.ExecuteAsync(sql, parameters, transaction, commandTimeout, CommandType.Text);
		}
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Insert{T}(IDbConnection, T, IDbTransaction, int?)"/>.
		/// </summary>
		public async Task InsertAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();

			var columns = classMap.GetMutableColumns();

			var autoGeneratedProperty = classMap.GetAutoGeneratedId();

			var guids = new IPropertyMap[0];

			if (Configuration.AutoPopulateKeyGuidValue)
			{
				guids = columns.Where(x => x.PropertyInfo.PropertyType == typeof(Guid)).ToArray();

				foreach (var guid in guids)
				{
					if ((Guid)guid.Getter(entity) != Guid.Empty)
						continue;

					var value = Configuration.GuidCreator.GetGuid();
					guid.Setter(entity, value);
				}
			}

			var sql = SqlGenerator.Insert(classMap);

			var dynamicParameters = new DynamicParameters();

			foreach (var column in columns)
			{
				dynamicParameters.Add(column.Name, column.Getter(entity));
			}

			if (autoGeneratedProperty != null)
			{
				IEnumerable<long> result;
				if (SqlGenerator.SupportsMultipleStatements())
				{
					sql += Configuration.Dialect.BatchSeparator + SqlGenerator.IdentitySql(classMap);
					result = await connection.QueryAsync<long>(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
				}
				else
				{
					await connection.ExecuteAsync(sql, entity, transaction, commandTimeout, CommandType.Text);
					sql = SqlGenerator.IdentitySql(classMap);
					result = await connection.QueryAsync<long>(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
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

			await connection.ExecuteAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
		}
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Update{T}(IDbConnection, T, IDbTransaction, int?)"/>.
		/// </summary>
		public async Task<bool> UpdateAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var predicate = classMap.GetKeyPredicate(entity);
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Update(classMap, predicate, parameters);
			var dynamicParameters = new DynamicParameters();

			var columns = classMap.GetMutableColumns();

			foreach (var property in ReflectionHelper.GetObjectValues(entity)
				.Where(property => columns.Any(c => c.Name == property.Key)))
			{
				dynamicParameters.Add(property.Key, property.Value);
			}

			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return await connection.ExecuteAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
		}
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Delete{T}(IDbConnection, T, IDbTransaction, int?)"/>.
		/// </summary>
		public async Task<bool> DeleteAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var predicate = classMap.GetKeyPredicate(entity);
			return await DeleteAsync<T>(connection, classMap, predicate, transaction, commandTimeout);
		}
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Delete{T}(IDbConnection, object, IDbTransaction, int?)"/>.
		/// </summary>
		public async Task<bool> DeleteAsync<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var wherePredicate = classMap.GetPredicate(predicate);
			return await DeleteAsync<T>(connection, classMap, wherePredicate, transaction, commandTimeout);
		}

		protected async Task<bool> DeleteAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Delete(classMap, predicate, parameters);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return await connection.ExecuteAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
		}
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Get{T}"/>.
		/// </summary>
		public async Task<T> GetAsync<T>(IDbConnection connection,
			object id,
			IDbTransaction transaction = null,
			int? commandTimeout = null) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var predicate = classMap.GetIdPredicate(id);
			var result = (await GetListAsync<T>(connection, classMap, predicate, null, transaction, commandTimeout, null)).SingleOrDefault();
			return result;
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetList{T}"/>.
		/// </summary>
		public async Task<IEnumerable<T>> GetListAsync<T>(IDbConnection connection,
			object predicate = null,
			IList<ISort> sort = null,
			IDbTransaction transaction = null,
			int? commandTimeout = null,
			IList<IProjection> projections = null) where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			var wherePredicate = classMap.GetPredicate(predicate);
			return await GetListAsync<T>(connection, classMap, wherePredicate, sort, transaction, commandTimeout, projections);
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetPage{T}"/>.
		/// </summary>
		public async Task<IEnumerable<T>> GetPageAsync<T>(
			IDbConnection connection,
			object predicate = null,
			IList<ISort> sort = null,
			int page = 1,
			int resultsPerPage = 10,
			IDbTransaction transaction = null,
			int? commandTimeout = null,
			IList<IProjection> projections = null) where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			var wherePredicate = classMap.GetPredicate(predicate);
			return await GetPageAsync<T>(connection, classMap, wherePredicate, sort, page, resultsPerPage, transaction, commandTimeout, projections);
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetSet{T}"/>.
		/// </summary>
		public async Task<IEnumerable<T>> GetSetAsync<T>(IDbConnection connection,
			object predicate = null,
			IList<ISort> sort = null,
			int firstResult = 1,
			int maxResults = 10,
			IDbTransaction transaction = null,
			int? commandTimeout = null,
			IList<IProjection> projections = null) where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			var wherePredicate = classMap.GetPredicate(predicate);
			return await GetSetAsync<T>(connection, classMap, wherePredicate, sort, firstResult, maxResults, transaction, commandTimeout, projections);
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Count{T}"/>.
		/// </summary>
		public async Task<int> CountAsync<T>(IDbConnection connection, object predicate = null, IDbTransaction transaction = null,
			int? commandTimeout = null) where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			var wherePredicate = classMap.GetPredicate(predicate);
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Count(classMap, wherePredicate, parameters);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return (int)(await connection.QueryAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text)).Single().Total;
		}

		public async Task<int> AtomicIncrementAsync<T>(IDbConnection connection,
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
				throw new InvalidOperationException($"Atomic increment is not allowed on {projection.PropertyName} for type {classMap.EntityType}. It's either ignored or read only");

			var wherePredicate = classMap.GetPredicate(predicate);
			var parameters = new Dictionary<string, object>();

			var sql = SqlGenerator.AtomicIncrement(classMap, wherePredicate, parameters, projection, amount);

			return await connection.ExecuteAsync(sql, parameters, dbTransaction, commandTimeout, CommandType.Text);
		}

		#endregion

		#region Helpers

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetList{T}"/>.
		/// </summary>
		protected async Task<IEnumerable<T>> GetListAsync<T>(
			IDbConnection connection,
			IClassMapper classMap,
			IPredicate predicate,
			IList<ISort> sort,
			IDbTransaction transaction,
			int? commandTimeout,
			IList<IProjection> projections) where T : class
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Select(classMap, predicate, sort, parameters, projections);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return await connection.QueryAsync<T>(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetPage{T}"/>.
		/// </summary>
		protected async Task<IEnumerable<T>> GetPageAsync<T>(
			IDbConnection connection,
			IClassMapper classMap,
			IPredicate predicate,
			IList<ISort> sort,
			int page,
			int resultsPerPage,
			IDbTransaction transaction,
			int? commandTimeout,
			IList<IProjection> projections) where T : class
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.SelectPaged(classMap, predicate, sort, page, resultsPerPage, parameters, projections);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return await connection.QueryAsync<T>(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetSet{T}"/>.
		/// </summary>
		protected async Task<IEnumerable<T>> GetSetAsync<T>(
			IDbConnection connection,
			IClassMapper classMap,
			IPredicate predicate,
			IList<ISort> sort,
			int firstResult,
			int maxResults,
			IDbTransaction transaction,
			int? commandTimeout,
			IList<IProjection> projections) where T : class
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.SelectSet(classMap, predicate, sort, firstResult, maxResults, parameters, projections);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return await connection.QueryAsync<T>(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
		}

		#endregion
	}
}