using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Predicates;

namespace ExistsForAll.DapperExtensions.Sql
{
	public class SqlGenerator : ISqlGenerator
	{
		private IDapperExtensionsConfiguration Configuration { get; }
		private readonly ConcurrentDictionary<RuntimeTypeHandle,string> _selectCache = new ConcurrentDictionary<RuntimeTypeHandle, string>();
		private readonly ConcurrentDictionary<RuntimeTypeHandle, string> _insertCache = new ConcurrentDictionary<RuntimeTypeHandle, string>();
		private readonly ConcurrentDictionary<RuntimeTypeHandle, string> _updateCache = new ConcurrentDictionary<RuntimeTypeHandle, string>();

		public SqlGenerator(IDapperExtensionsConfiguration configuration)
		{
			Configuration = configuration;
		}

		private string GetSelectQuery(IClassMapper map, ICollection<IProjection> projections)
		{
			if (projections != null)
				return $"SELECT {BuildSelectColumns(map, projections)} FROM {GetTableName(map)}";

			return _selectCache.GetOrAdd(map.EntityType.TypeHandle, $"SELECT {BuildSelectColumns(map, null)} FROM {GetTableName(map)}");
		}

		public string Select(
			IClassMapper map,
			IPredicate predicate,
			IList<ISort> sort,
			IDictionary<string, object> parameters,
			IList<IProjection> projections)
		{
			Guard.ArgumentNull(parameters, nameof(parameters));

			var context = new SqlGenerationContext(Configuration.Dialect, map);

			var selectQuery = GetSelectQuery(map, projections);

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
			IDictionary<string, object> parameters,
			IList<IProjection> projections)
		{
			Guard.EnumerableArgumentNull(sort, nameof(sort));
			Guard.ArgumentNull(parameters, nameof(parameters));

			var context = new SqlGenerationContext(Configuration.Dialect, classMap);

			var selectQuery = GetSelectQuery(classMap, projections);

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
			IDictionary<string, object> parameters,
			IList<IProjection> projections)
		{
			Guard.EnumerableArgumentNull(sort, nameof(sort));
			Guard.ArgumentNull(parameters, nameof(parameters));

			var context = new SqlGenerationContext(Configuration.Dialect, classMap);

			var selectQuery = GetSelectQuery(classMap, projections);

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

			var countSql = $"SELECT COUNT(*) AS {Configuration.Dialect.WrapWithQuotes("Total")} FROM {GetTableName(classMap)}";

			var sql = new StringBuilder(countSql);

			if (predicate != null)
			{
				sql.Append(" WHERE ")
					.Append(predicate.GetSql(context, parameters));
			}

			return sql.ToString();
		}

		public string Insert(IClassMapper classMap)
		{
			return _insertCache.GetOrAdd(classMap.EntityType.TypeHandle, x =>
			{
				var columns = classMap.GetMutableColumns();

				if (!columns.Any())
					throw new ArgumentException("No columns were mapped.");

				var columnNames = columns.Select(p => GetColumnName(classMap, p, false));

				var parameters = columns.Select(p => Configuration.Dialect.ParameterPrefix + p.Name);

				var sql = new StringBuilder(
					$"INSERT INTO {GetTableName(classMap)} ({columnNames.AppendStrings()}) VALUES ({parameters.AppendStrings()})");

				return sql.ToString();
			});
		}

		public string Update(IClassMapper classMap,
			IPredicate predicate,
			IDictionary<string, object> parameters)
		{
			Guard.ArgumentNull(predicate, nameof(predicate));
			Guard.ArgumentNull(parameters, nameof(parameters));

			var context = new SqlGenerationContext(Configuration.Dialect, classMap);

			var setSql = _updateCache.GetOrAdd(classMap.EntityType.TypeHandle, x =>
			{
				var columns = classMap.GetMutableColumns();

				if (!columns.Any())
					throw new ArgumentException("No columns were mapped.");

				return columns.Select(
					p => $"{GetColumnName(classMap, p, false)} = {Configuration.Dialect.ParameterPrefix}{p.Name}")
					.AppendStrings();
			});

			return $"UPDATE {GetTableName(classMap)} SET {setSql} WHERE {predicate.GetSql(context, parameters)}";
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

		public string AtomicIncrement(IClassMapper map, IPredicate predicate, IDictionary<string, object> parameters, IProjection projection, int amount)
		{
			var context = new SqlGenerationContext(Configuration.Dialect, map);

			var column = GetColumnName(map,projection.PropertyName, false);

			return $"UPDATE {GetTableName(map)} SET {column} = {column} + {amount} WHERE {predicate.GetSql(context, parameters)}";
		}

		public virtual bool SupportsMultipleStatements()
		{
			return Configuration.Dialect.SupportsMultipleStatements;
		}

		public string IdentitySql(IClassMapper classMap)
		{
			return classMap.IdentitySql(Configuration.Dialect);
		}

		protected string GetTableName(IClassMapper map)
		{
			return map.GetTableName(Configuration.Dialect);
		}

		protected string GetColumnName(IClassMapper map, IPropertyMap property, bool includeAlias)
		{
			string alias = null;
			if (property.ColumnName != property.Name && includeAlias)
			{
				alias = property.Name;
			}

			return Configuration.Dialect.GetColumnName(map.GetTableName(Configuration.Dialect), property.ColumnName, alias);
		}

		protected string GetColumnName(IClassMapper map, string propertyName, bool includeAlias)
		{
			var propertyMap = map.GetJoinedMapByName(propertyName);

			if (propertyMap == null)
				throw new ArgumentException(string.Format("Could not find '{0}' in Mapping.", propertyName));
			
			return GetColumnName(map, propertyMap, includeAlias);
		}

		protected string BuildSelectColumns(IClassMapper classMap, ICollection<IProjection> projections)
		{
			var columns = classMap.Keys
				.Concat(classMap.Properties.Where(x => !x.Ignored));

			if (projections != null)
			{
				var projectedProperties = projections.Select(x => x.PropertyName).ToArray();
				columns = columns.Where(c => projectedProperties.Contains(c.PropertyInfo.Name));
			}

			return string.Join(", ", columns.Select(x => GetColumnName(classMap, x, true)));
		}
	}
}