﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public class DapperImplementor : IDapperImplementor
	{
		private IClassMapperRepository ClassMappers { get; }
		private IDapperExtensionsConfiguration Configuration { get; }

		public DapperImplementor(ISqlGenerator sqlGenerator,
			IClassMapperRepository classMappers,
			IDapperExtensionsConfiguration dapperExtensionsConfiguration)
		{
			ClassMappers = classMappers;
			Configuration = dapperExtensionsConfiguration;
			SqlGenerator = sqlGenerator;
		}

		private ISqlGenerator SqlGenerator { get; }

		public T Get<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			IPredicate predicate = GetIdPredicate(classMap, id);
			var result = GetList<T>(connection, classMap, predicate, null, transaction, commandTimeout, true).SingleOrDefault();
			return result;
		}

		public void Insert<T>(IDbConnection connection,
			IEnumerable<T> entities,
			IDbTransaction transaction,
			int? commandTimeout) where T : class
		{

			IEnumerable<PropertyInfo> properties = null;

			var classMap = ClassMappers.GetMap<T>();

			var keys = classMap.Keys;

			var triggerIdentityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.TriggerIdentity);

			var parameters = new List<DynamicParameters>();
			
			if (triggerIdentityColumn != null)
			{
				properties = typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
					.Where(p => p.Name != triggerIdentityColumn.PropertyInfo.Name);
			}

			foreach (var e in entities)
			{
				foreach (var column in keys)
				{
					// todo : deal with GUID self generated. 
					if (column.KeyType == KeyType.Guid && (Guid) column.PropertyInfo.GetValue(e, null) == Guid.Empty)
					{
						Guid comb = Configuration.GetNextGuid();
						column.PropertyInfo.SetValue(e, comb, null);
					}
				}

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
				connection.Execute(sql, entities, transaction, commandTimeout, CommandType.Text);
			}
			else
			{
				connection.Execute(sql, parameters, transaction, commandTimeout, CommandType.Text);
			}
		}

		public dynamic Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout)
			where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			var nonIdentityKeyProperties =
				classMap.Properties.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned).ToList();
			var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);
			var triggerIdentityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.TriggerIdentity);

			foreach (var column in nonIdentityKeyProperties)
			{
				if (column.KeyType == KeyType.Guid && (Guid) column.PropertyInfo.GetValue(entity, null) == Guid.Empty)
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

				// We are only interested in the first identity, but we are iterating over all resulting items (if any).
				// This makes sure that ADO.NET drivers (like MySql) won't actively terminate the query.
				var hasResult = false;
				var identityInt = 0;
				foreach (var identityValue in result)
				{
					if (hasResult)
					{
						continue;
					}
					identityInt = Convert.ToInt32(identityValue);
					hasResult = true;
				}
				if (!hasResult)
				{
					throw new InvalidOperationException("The source sequence is empty.");
				}

				keyValues.Add(identityColumn.Name, identityInt);
				identityColumn.PropertyInfo.SetValue(entity, identityInt, null);
			}
			else if (triggerIdentityColumn != null)
			{
				var dynamicParameters = new DynamicParameters();
				foreach (var prop in entity.GetType()
					.GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
					.Where(p => p.Name != triggerIdentityColumn.PropertyInfo.Name))
				{
					dynamicParameters.Add(prop.Name, prop.GetValue(entity, null));
				}

				// defaultValue need for identify type of parameter
				var defaultValue = entity.GetType().GetProperty(triggerIdentityColumn.PropertyInfo.Name).GetValue(entity, null);
				dynamicParameters.Add("IdOutParam", direction: ParameterDirection.Output, value: defaultValue);

				connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);

				var value = dynamicParameters.Get<object>(Configuration.Dialect.ParameterPrefix + "IdOutParam");
				keyValues.Add(triggerIdentityColumn.Name, value);
				triggerIdentityColumn.PropertyInfo.SetValue(entity, value, null);
			}
			else
			{
				connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
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

		public bool Update<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout,
			bool ignoreAllKeyProperties = false) where T : class
		{
			var classMap = ClassMappers.GetMap<T>();
			var predicate = GetKeyPredicate<T>(classMap, entity);
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Update(classMap, predicate, parameters, ignoreAllKeyProperties);
			var dynamicParameters = new DynamicParameters();

			var columns = classMap.GetMutableColumns();

			foreach (var property in XExtensions.GetObjectValues(entity)
				.Where(property => columns.Any(c => c.Name == property.Key)))
			{
				dynamicParameters.Add(property.Key, property.Value);
			}

			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
		}

		public bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout)
			where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			var predicate = GetKeyPredicate<T>(classMap, entity);
			return Delete<T>(connection, classMap, predicate, transaction, commandTimeout);
		}

		public bool Delete<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout)
			where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			var wherePredicate = GetPredicate(classMap, predicate);
			return Delete<T>(connection, classMap, wherePredicate, transaction, commandTimeout);
		}

		public IEnumerable<T> GetList<T>(IDbConnection connection, object predicate, IList<ISort> sort,
			IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			var wherePredicate = GetPredicate(classMap, predicate);
			return GetList<T>(connection, classMap, wherePredicate, sort, transaction, commandTimeout, buffered);
		}

		public IEnumerable<T> GetPage<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page,
			int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			var wherePredicate = GetPredicate(classMap, predicate);
			return GetPage<T>(connection, classMap, wherePredicate, sort, page, resultsPerPage, transaction, commandTimeout,
				buffered);
		}

		public IEnumerable<T> GetSet<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult,
			int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
		{
			IClassMapper classMap = ClassMappers.GetMap<T>();
			var wherePredicate = GetPredicate(classMap, predicate);
			return GetSet<T>(connection, classMap, wherePredicate, sort, firstResult, maxResults, transaction, commandTimeout,
				buffered);
		}

		public int Count<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout)
			where T : class
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

			return (int) connection.Query(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text).Single()
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

		protected IEnumerable<T> GetList<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate,
			IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Select(classMap, predicate, sort, parameters);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
		}

		protected IEnumerable<T> GetPage<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate,
			IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered)
			where T : class
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.SelectPaged(classMap, predicate, sort, page, resultsPerPage, parameters);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
		}

		protected IEnumerable<T> GetSet<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate,
			IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered)
			where T : class
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.SelectSet(classMap, predicate, sort, firstResult, maxResults, parameters);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
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

		protected IPredicate GetPredicate(IClassMapper classMap, object predicate)
		{
			var wherePredicate = predicate as IPredicate;
			if (wherePredicate == null && predicate != null)
			{
				wherePredicate = GetEntityPredicate(classMap, predicate);
			}

			return wherePredicate;
		}

		protected IPredicate GetIdPredicate(IClassMapper classMap, object id)
		{
			var isSimpleType = id.GetType().IsSimpleType();

			var keys = classMap.Keys;

			IDictionary<string, object> paramValues = null;

			IList<IPredicate> predicates = new List<IPredicate>();

			if (!isSimpleType)
			{
				paramValues = XExtensions.GetObjectValues(id);
			}

			foreach (var key in keys)
			{
				var value = id;
				if (!isSimpleType)
				{
					value = paramValues[key.Name];
				}

				var predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);

				var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
				fieldPredicate.Not = false;
				fieldPredicate.Operator = Operator.Eq;
				fieldPredicate.PropertyName = key.Name;
				fieldPredicate.Value = value;
				predicates.Add(fieldPredicate);
			}

			return predicates.Count == 1
				? predicates[0]
				: new PredicateGroup
				{
					Operator = GroupOperator.And,
					Predicates = predicates
				};
		}

		protected IPredicate GetKeyPredicate<T>(IClassMapper classMap, T entity) where T : class
		{
			var whereFields = classMap.Keys;

			if (!whereFields.Any())
			{
				throw new ArgumentException("At least one Key column must be defined.");
			}

			IList<IPredicate> predicates = (from field in whereFields
				select new FieldPredicate<T>
				{
					Not = false,
					Operator = Operator.Eq,
					PropertyName = field.Name,
					Value = field.PropertyInfo.GetValue(entity, null)
				}).Cast<IPredicate>().ToList();

			return predicates.Count == 1
				? predicates[0]
				: new PredicateGroup
				{
					Operator = GroupOperator.And,
					Predicates = predicates
				};
		}

		protected IPredicate GetEntityPredicate(IClassMapper classMap, object entity)
		{
			var predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);
			IList<IPredicate> predicates = new List<IPredicate>();
			foreach (var kvp in XExtensions.GetObjectValues(entity))
			{
				var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
				fieldPredicate.Not = false;
				fieldPredicate.Operator = Operator.Eq;
				fieldPredicate.PropertyName = kvp.Key;
				fieldPredicate.Value = kvp.Value;
				predicates.Add(fieldPredicate);
			}

			return predicates.Count == 1
				? predicates[0]
				: new PredicateGroup
				{
					Operator = GroupOperator.And,
					Predicates = predicates
				};
		}

		protected GridReaderResultReader GetMultipleByBatch(IDbConnection connection, GetMultiplePredicate predicate,
			IDbTransaction transaction, int? commandTimeout)
		{
			var parameters = new Dictionary<string, object>();
			var sql = new StringBuilder();
			foreach (var item in predicate.Items)
			{
				IClassMapper classMap = ClassMappers.GetMap(item.Type);
				var itemPredicate = item.Value as IPredicate;
				if (itemPredicate == null && item.Value != null)
				{
					itemPredicate = GetPredicate(classMap, item.Value);
				}

				sql.AppendLine(SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters) +
				               Configuration.Dialect.BatchSeperator);
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
				IClassMapper classMap = ClassMappers.GetMap(item.Type);
				var itemPredicate = item.Value as IPredicate;
				if (itemPredicate == null && item.Value != null)
				{
					itemPredicate = GetPredicate(classMap, item.Value);
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
