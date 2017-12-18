﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.Sql
{
	internal class SqlGenerator : ISqlGenerator
	{
		private IDapperExtensionsConfiguration Configuration { get; }
		private readonly ConcurrentDictionary<RuntimeTypeHandle,string> _selectCache = new ConcurrentDictionary<RuntimeTypeHandle, string>();

		public SqlGenerator(IDapperExtensionsConfiguration configuration)
		{
			Configuration = configuration;
		}

		private string GetCacheSelectQuery(IClassMapper map)
		{
			return _selectCache.GetOrAdd(map.EntityType.TypeHandle, $"SELECT {BuildSelectColumns(map)} FROM {GetTableName(map)}");
		}

		public string Select(IClassMapper map, IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters)
		{
			Guard.ArgumentNull(parameters, nameof(parameters));

			var context = new SqlGenerationContext(Configuration.Dialect, map);

			var selectQuery = GetCacheSelectQuery(map);

			var sql = new StringBuilder(selectQuery);

			if (predicate != null)
			{
				sql.Append(" WHERE ")
					.Append(predicate.GetSql(context, parameters));
			}

			if (sort != null && sort.Any())
			{
				sql.Append(" ORDER BY ")
					.Append(sort.Select(s => GetColumnName(map, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings());
			}

			return sql.ToString();
		}

		public string SelectPaged(IClassMapper classMap, IPredicate predicate,
			IList<ISort> sort,
			int page,
			int resultsPerPage,
			IDictionary<string, object> parameters)
		{
			Guard.EnumerableArgumentNull(sort, nameof(sort));
			Guard.ArgumentNull(parameters, nameof(parameters));

			var context = new SqlGenerationContext(Configuration.Dialect, classMap);

			var selectQuery = GetCacheSelectQuery(classMap);

			var innerSql = new StringBuilder(selectQuery);

			if (predicate != null)
			{
				innerSql.Append(" WHERE ")
					.Append(predicate.GetSql(context, parameters));
			}

			var orderBy = sort.Select(s => GetColumnName(classMap, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings();
			innerSql.Append(" ORDER BY " + orderBy);

			var sql = Configuration.Dialect.GetPagingSql(innerSql.ToString(), page, resultsPerPage, parameters);
			return sql;
		}

		public string SelectSet(IClassMapper classMap, IPredicate predicate,
			IList<ISort> sort,
			int firstResult,
			int maxResults,
			IDictionary<string, object> parameters)
		{
			Guard.EnumerableArgumentNull(sort, nameof(sort));
			Guard.ArgumentNull(parameters, nameof(parameters));

			var context = new SqlGenerationContext(Configuration.Dialect, classMap);

			var selectQuery = GetCacheSelectQuery(classMap);

			var innerSql = new StringBuilder(selectQuery);

			if (predicate != null)
			{
				innerSql.Append(" WHERE ")
					.Append(predicate.GetSql(context, parameters));
			}

			var orderBy = sort.Select(s => GetColumnName(classMap, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings();
			innerSql.Append(" ORDER BY " + orderBy);

			var sql = Configuration.Dialect.GetSetSql(innerSql.ToString(), firstResult, maxResults, parameters);
			return sql;
		}

		public string Count(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters)
		{
			Guard.ArgumentNull(parameters, nameof(parameters));

			var context = new SqlGenerationContext(Configuration.Dialect, classMap);

			var sql = new StringBuilder(string.Format("SELECT COUNT(*) AS {0}Total{1} FROM {2}",
								Configuration.Dialect.OpenQuote,
								Configuration.Dialect.CloseQuote,
								GetTableName(classMap)));
			if (predicate != null)
			{
				sql.Append(" WHERE ")
					.Append(predicate.GetSql(context, parameters));
			}

			return sql.ToString();
		}

		public string Insert(IClassMapper classMap)
		{
			var columns = classMap.GetMutableColumns();

			if (!columns.Any())
				throw new ArgumentException("No columns were mapped.");

			var columnNames = columns.Select(p => GetColumnName(classMap, p, false));

			var parameters = columns.Select(p => Configuration.Dialect.ParameterPrefix + p.Name);

			var sql = $"INSERT INTO {GetTableName(classMap)} ({columnNames.AppendStrings()}) VALUES ({parameters.AppendStrings()})";

			var autoGeneratedPropertyId = classMap.GetAutoGeneratedId();

			if (autoGeneratedPropertyId != null)
			{
				sql += $" RETURNING {GetColumnName(classMap, autoGeneratedPropertyId, false)} INTO {Configuration.Dialect.ParameterPrefix}IdOutParam";
			}

			return sql;
		}

		public string InsertBatch(IClassMapper classMap)
		{
			var columns = classMap.GetMutableColumns();

			if (!columns.Any())
				throw new ArgumentException("No columns were mapped.");

			var columnNames = columns.Select(p => GetColumnName(classMap, p, false));

			var parameters = columns.Select(p => Configuration.Dialect.ParameterPrefix + p.Name);

			var sql = $"INSERT INTO {GetTableName(classMap)} ({columnNames.AppendStrings()}) VALUES ({parameters.AppendStrings()})";

			var autoGeneratedPropertyId = classMap.GetAutoGeneratedId();

			if (autoGeneratedPropertyId != null)
			{
				sql += $" RETURNING {GetColumnName(classMap, autoGeneratedPropertyId, false)} INTO {Configuration.Dialect.ParameterPrefix}IdOutParam";
			}

			return sql;
		}

		public string Update(IClassMapper classMap, IPredicate predicate,
			IDictionary<string, object> parameters,
			bool ignoreAllKeyProperties)
		{
			Guard.ArgumentNull(predicate, nameof(predicate));
			Guard.ArgumentNull(parameters, nameof(parameters));

			var context = new SqlGenerationContext(Configuration.Dialect, classMap);

			var columns = classMap.GetMutableColumns();

			if (!columns.Any())
				throw new ArgumentException("No columns were mapped.");

			var setSql = columns.Select(
					p => $"{GetColumnName(classMap, p, false)} = {Configuration.Dialect.ParameterPrefix}{p.Name}");

			return $"UPDATE {GetTableName(classMap)} SET {setSql.AppendStrings()} WHERE {predicate.GetSql(context, parameters)}";
		}

		public string Delete(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters)
		{
			Guard.ArgumentNull(predicate, nameof(predicate));
			Guard.ArgumentNull(parameters, nameof(parameters));

			var context = new SqlGenerationContext(Configuration.Dialect, classMap);

			var sql = new StringBuilder($"DELETE FROM {GetTableName(classMap)}");
			sql.Append(" WHERE ").Append(predicate.GetSql(context, parameters));
			return sql.ToString();
		}

		public virtual bool SupportsMultipleStatements()
		{
			return Configuration.Dialect.SupportsMultipleStatements;
		}

		public string IdentitySql(IClassMapper classMap)
		{
			return classMap.IdentitySql(Configuration.Dialect);
		}

		private string GetTableName(IClassMapper map)
		{
			return map.GetTableName(Configuration.Dialect);
		}

		private string GetColumnName(IClassMapper map, IPropertyMap property, bool includeAlias)
		{
			string alias = null;
			if (property.ColumnName != property.Name && includeAlias)
			{
				alias = property.Name;
			}

			return Configuration.Dialect.GetColumnName(map.GetTableName(Configuration.Dialect), property.ColumnName, alias);
		}

		private string GetColumnName(IClassMapper map, string propertyName, bool includeAlias)
		{
			var propertyMap = map.GetJoinedMapByName(propertyName);

			if (propertyMap == null)
				throw new ArgumentException(string.Format("Could not find '{0}' in Mapping.", propertyName));
			
			return GetColumnName(map, propertyMap, includeAlias);
		}

		private string BuildSelectColumns(IClassMapper classMap)
		{
			var columns = classMap.Keys.Where(x => !x.Ignored)
				.Concat(classMap.Properties.Where(x => !x.Ignored))
				.Select(x => GetColumnName(classMap, x, true));

			return string.Join(", ", columns);
		}
	}
}