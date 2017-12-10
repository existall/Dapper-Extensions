﻿/*
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public class DapperAsyncImplementor : DapperImplementor, IDapperAsyncImplementor
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
			: base(sqlGenerator, classMappers, dapperExtensionsConfiguration)
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
			IEnumerable<PropertyInfo> properties = null;

			var classMap = ClassMappers.GetMap<T>();


			// todo: replace this logic
			var triggerIdentityColumn = classMap.Keys.SingleOrDefault(p => p.KeyType == KeyType.TriggerIdentity);

			var parameters = new List<DynamicParameters>();

			if (triggerIdentityColumn != null)
			{
				properties = typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
					.Where(p => p.Name != triggerIdentityColumn.PropertyInfo.Name);
			}

			foreach (var e in entities)
			{
				if (triggerIdentityColumn != null)
				{
					var dynamicParameters = new DynamicParameters();
					foreach (var prop in properties)
					{
						dynamicParameters.Add(prop.Name, prop.GetValue(e, null));
					}

					// defaultValue need for identify type of parameter
					var defaultValue = typeof(T).GetProperty(triggerIdentityColumn.PropertyInfo.Name).GetValue(e, null);
					dynamicParameters.Add("IdOutParam", direction: ParameterDirection.Output, value: defaultValue);

					parameters.Add(dynamicParameters);
				}
			}

			var sql = SqlGenerator.Insert(classMap);

			if (triggerIdentityColumn == null)
			{
				await connection.ExecuteAsync(sql, entities, transaction, commandTimeout, CommandType.Text);
			}
			else
			{
				await connection.ExecuteAsync(sql, parameters, transaction, commandTimeout, CommandType.Text);
			}
		}
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Insert{T}(IDbConnection, T, IDbTransaction, int?)"/>.
		/// </summary>
		public async Task<dynamic> InsertAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			var nonIdentityKeyProperties = classMap.Properties.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned).ToList();
			var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);
			var triggerIdentityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.TriggerIdentity);
			foreach (var column in nonIdentityKeyProperties)
			{
				if (column.KeyType == KeyType.Guid && (Guid)column.PropertyInfo.GetValue(entity, null) == Guid.Empty)
				{
					Guid comb = Configuration.GetNextGuid();
					column.PropertyInfo.SetValue(entity, comb, null);
				}
			}

			IDictionary<string, object> keyValues = new ExpandoObject();
			var sql = SqlGenerator.Insert(classMap);
			if (identityColumn != null)
			{
				IEnumerable<long> result;
				if (SqlGenerator.SupportsMultipleStatements())
				{
					sql += Configuration.Dialect.BatchSeperator + SqlGenerator.IdentitySql(classMap);
					result = connection.Query<long>(sql, entity, transaction, false, commandTimeout, CommandType.Text);
				}
				else
				{
					connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
					sql = SqlGenerator.IdentitySql(classMap);
					result = connection.Query<long>(sql, entity, transaction, false, commandTimeout, CommandType.Text);
				}

				var identityValue = result.First();
				var identityInt = Convert.ToInt32(identityValue);
				keyValues.Add(identityColumn.Name, identityInt);
				identityColumn.PropertyInfo.SetValue(entity, identityInt, null);
			}
			else if (triggerIdentityColumn != null)
			{
				var dynamicParameters = new DynamicParameters();
				foreach (var prop in entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
					.Where(p => p.Name != triggerIdentityColumn.PropertyInfo.Name))
				{
					dynamicParameters.Add(prop.Name, prop.GetValue(entity, null));
				}

				// defaultValue need for identify type of parameter
				var defaultValue = entity.GetType().GetProperty(triggerIdentityColumn.PropertyInfo.Name).GetValue(entity, null);
				dynamicParameters.Add("IdOutParam", direction: ParameterDirection.Output, value: defaultValue);

				await connection.ExecuteAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text).ConfigureAwait(false);

				var value = dynamicParameters.Get<object>(Configuration.Dialect.ParameterPrefix + "IdOutParam");
				keyValues.Add(triggerIdentityColumn.Name, value);
				triggerIdentityColumn.PropertyInfo.SetValue(entity, value, null);
			}
			else
			{
				await connection.ExecuteAsync(sql, entity, transaction, commandTimeout, CommandType.Text).ConfigureAwait(false);
			}

			foreach (var column in nonIdentityKeyProperties)
			{
				keyValues.Add(column.Name, column.PropertyInfo.GetValue(entity, null));
			}

			if (keyValues.Count == 1)
			{
				return keyValues.First().Value;
			}

			return keyValues;
		}
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Update{T}(IDbConnection, T, IDbTransaction, int?)"/>.
		/// </summary>
		public async Task<bool> UpdateAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var predicate = GetKeyPredicate<T>(classMap, entity);
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Update(classMap, predicate, parameters, ignoreAllKeyProperties);
			var dynamicParameters = new DynamicParameters();

			var columns = classMap.GetMutableColumns();

			foreach (var property in XExtensions.GetObjectValues(entity).Where(property => columns.Any(c => c.Name == property.Key)))
			{
				dynamicParameters.Add(property.Key, property.Value);
			}

			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return await connection.ExecuteAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text).ConfigureAwait(false) > 0;
		}
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Delete{T}(IDbConnection, T, IDbTransaction, int?)"/>.
		/// </summary>
		public async Task<bool> DeleteAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var predicate = GetKeyPredicate(classMap, entity);
			return await DeleteAsync<T>(connection, classMap, predicate, transaction, commandTimeout);
		}
		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Delete{T}(IDbConnection, object, IDbTransaction, int?)"/>.
		/// </summary>
		public async Task<bool> DeleteAsync<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var wherePredicate = GetPredicate(classMap, predicate);
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
		public async Task<T> GetAsync<T>(IDbConnection connection, dynamic id, IDbTransaction transaction = null,
			int? commandTimeout = null) where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			IPredicate predicate = GetIdPredicate(classMap, id);
			return (await GetListAsync<T>(connection, classMap, predicate, null, transaction, commandTimeout)).SingleOrDefault();
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetList{T}"/>.
		/// </summary>
		public async Task<IEnumerable<T>> GetListAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null,
			IDbTransaction transaction = null, int? commandTimeout = null) where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			var wherePredicate = GetPredicate(classMap, predicate);
			return await GetListAsync<T>(connection, classMap, wherePredicate, sort, transaction, commandTimeout);
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetPage{T}"/>.
		/// </summary>
		public async Task<IEnumerable<T>> GetPageAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int page = 1,
			int resultsPerPage = 10, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			var wherePredicate = GetPredicate(classMap, predicate);
			return await GetPageAsync<T>(connection, classMap, wherePredicate, sort, page, resultsPerPage, transaction, commandTimeout);
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetSet{T}"/>.
		/// </summary>
		public async Task<IEnumerable<T>> GetSetAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int firstResult = 1,
			int maxResults = 10, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			var wherePredicate = GetPredicate(classMap, predicate);
			return await GetSetAsync<T>(connection, classMap, wherePredicate, sort, firstResult, maxResults, transaction, commandTimeout);
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Count{T}"/>.
		/// </summary>
		public async Task<int> CountAsync<T>(IDbConnection connection, object predicate = null, IDbTransaction transaction = null,
			int? commandTimeout = null) where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			var wherePredicate = GetPredicate(classMap, predicate);
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Count(classMap, wherePredicate, parameters);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return (int)(await connection.QueryAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text)).Single().Total;
		}

		#endregion

		#region Helpers

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetList{T}"/>.
		/// </summary>
		protected async Task<IEnumerable<T>> GetListAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Select(classMap, predicate, sort, parameters);
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
		protected async Task<IEnumerable<T>> GetPageAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.SelectPaged(classMap, predicate, sort, page, resultsPerPage, parameters);
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
		protected async Task<IEnumerable<T>> GetSetAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.SelectSet(classMap, predicate, sort, firstResult, maxResults, parameters);
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
*/
