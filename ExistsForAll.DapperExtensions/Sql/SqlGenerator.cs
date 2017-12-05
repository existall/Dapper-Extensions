using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.Sql
{
	internal class SqlGenerator : ISqlGenerator
	{
		private readonly IClassMapperRepository _classMapperRepository;
		private IDapperExtensionsConfiguration Configuration { get; }
		private ISqlGenerationContext Context { get; }

		public SqlGenerator(IDapperExtensionsConfiguration configuration,
			IClassMapperRepository classMapperRepository)
		{
			_classMapperRepository = classMapperRepository;
			Configuration = configuration;
			Context = new SqlGenerationContext(Configuration.Dialect,_classMapperRepository);
		}

		public string Select<T>(IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters) where T : IClassMapper
		{
			Guard.ArgumentNull(parameters, nameof(parameters));

			var classMap = _classMapperRepository.GetMap<T>();

			var sql = new StringBuilder(string.Format("SELECT {0} FROM {1}",
				BuildSelectColumns(classMap),
				GetTableName(classMap)));

			if (predicate != null)
			{
				sql.Append(" WHERE ")
					.Append(predicate.GetSql(Context, parameters));
			}

			if (sort != null && sort.Any())
			{
				sql.Append(" ORDER BY ")
					.Append(sort.Select(s => GetColumnName(classMap, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings());
			}

			return sql.ToString();
		}

		public string SelectPaged<T>(IPredicate predicate,
			IList<ISort> sort,
			int page,
			int resultsPerPage,
			IDictionary<string, object> parameters) where T : IClassMapper
		{
			Guard.EnumerableArgumentNull(sort, nameof(sort));
			Guard.ArgumentNull(parameters,nameof(parameters));

			var classMap = _classMapperRepository.GetMap<T>();

			var innerSql = new StringBuilder(string.Format("SELECT {0} FROM {1}",
				BuildSelectColumns(classMap),
				GetTableName(classMap)));
			if (predicate != null)
			{
				innerSql.Append(" WHERE ")
					.Append(predicate.GetSql(Context, parameters));
			}

			var orderBy = sort.Select(s => GetColumnName(classMap, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings();
			innerSql.Append(" ORDER BY " + orderBy);

			var sql = Configuration.Dialect.GetPagingSql(innerSql.ToString(), page, resultsPerPage, parameters);
			return sql;
		}

		public string SelectSet<T>(IPredicate predicate,
			IList<ISort> sort,
			int firstResult,
			int maxResults,
			IDictionary<string, object> parameters) where T : IClassMapper
		{
			Guard.EnumerableArgumentNull(sort, nameof(sort));
			Guard.ArgumentNull(parameters,nameof(parameters));

			var classMap = _classMapperRepository.GetMap<T>();

			var innerSql = new StringBuilder(string.Format("SELECT {0} FROM {1}",
				BuildSelectColumns(classMap),
				GetTableName(classMap)));
			if (predicate != null)
			{
				innerSql.Append(" WHERE ")
					.Append(predicate.GetSql(Context, parameters));
			}

			var orderBy = sort.Select(s => GetColumnName(classMap, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings();
			innerSql.Append(" ORDER BY " + orderBy);

			var sql = Configuration.Dialect.GetSetSql(innerSql.ToString(), firstResult, maxResults, parameters);
			return sql;
		}

		public string Count<T>(IPredicate predicate, IDictionary<string, object> parameters) where T : IClassMapper
		{
			Guard.ArgumentNull(parameters, nameof(parameters));

			var classMap = _classMapperRepository.GetMap<T>();

			var sql = new StringBuilder(string.Format("SELECT COUNT(*) AS {0}Total{1} FROM {2}",
								Configuration.Dialect.OpenQuote,
								Configuration.Dialect.CloseQuote,
								GetTableName(classMap)));
			if (predicate != null)
			{
				sql.Append(" WHERE ")
					.Append(predicate.GetSql(new SqlGenerationContext(Configuration.Dialect,_classMapperRepository), parameters));
			}

			return sql.ToString();
		}

		public string Insert<T>() where T : IClassMapper
		{
			var classMap = _classMapperRepository.GetMap<T>();

			var columns = classMap.GetMutableColumns();

			if (!columns.Any())
				throw new ArgumentException("No columns were mapped.");
			
			var columnNames = columns.Select(p => GetColumnName(classMap, p, false));
			var parameters = columns.Select(p => Configuration.Dialect.ParameterPrefix + p.Name);

			var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
									   GetTableName(classMap),
									   columnNames.AppendStrings(),
									   parameters.AppendStrings());

			var triggerIdentityColumn = classMap.GetTriggerIdentities();

			if (triggerIdentityColumn.Length > 0)
			{
				if (triggerIdentityColumn.Length > 1)
					throw new ArgumentException("TriggerIdentity generator cannot be used with multi-column keys");

				sql += string.Format(" RETURNING {0} INTO {1}IdOutParam", triggerIdentityColumn.Select(p => GetColumnName(classMap, p, false)).First(), Configuration.Dialect.ParameterPrefix);
			}

			return sql;
		}

		public string Update<T>(IPredicate predicate,
			IDictionary<string, object> parameters,
			bool ignoreAllKeyProperties) where T : IClassMapper
		{
			Guard.ArgumentNull(predicate, nameof(predicate));
			Guard.ArgumentNull(parameters, nameof(parameters));

			var classMap = _classMapperRepository.GetMap<T>();

			var columns = ignoreAllKeyProperties
				? classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly) && p.KeyType == KeyType.NotAKey).ToArray()
				: classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity || p.KeyType == KeyType.Assigned)).ToArray();

			if (!columns.Any())
			{
				throw new ArgumentException("No columns were mapped.");
			}

			var setSql =
				columns.Select(
					p =>
					string.Format(
						"{0} = {1}{2}", GetColumnName(classMap, p, false), Configuration.Dialect.ParameterPrefix, p.Name));

			return string.Format("UPDATE {0} SET {1} WHERE {2}",
				GetTableName(classMap),
				setSql.AppendStrings(),
				predicate.GetSql(Context, parameters));
		}

		public string Delete<T>(IPredicate predicate, IDictionary<string, object> parameters) where T : IClassMapper
		{
			Guard.ArgumentNull(predicate, nameof(predicate));
			Guard.ArgumentNull(parameters, nameof(parameters));

			var classMap = _classMapperRepository.GetMap<T>();

			var sql = new StringBuilder(string.Format("DELETE FROM {0}", GetTableName(classMap)));
			sql.Append(" WHERE ").Append(predicate.GetSql(Context, parameters));
			return sql.ToString();
		}

		public virtual bool SupportsMultipleStatements()
		{
			return Configuration.Dialect.SupportsMultipleStatements;
		}

		public string IdentitySql<T>()
		{
			var classMap = _classMapperRepository.GetMap<T>();

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
			var propertyMap = map.Properties.SingleOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

			if (propertyMap == null)
			{
				throw new ArgumentException(string.Format("Could not find '{0}' in Mapping.", propertyName));
			}

			return GetColumnName(map, propertyMap, includeAlias);
		}

		private string BuildSelectColumns(IClassMapper classMap)
		{
			var columns = classMap.Properties
				.Where(p => !p.Ignored)
				.Select(p => GetColumnName(classMap, p, true));
			return columns.AppendStrings();
		}
	}
}