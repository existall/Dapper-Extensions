using System;
using System.Collections.Generic;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public class ExistsPredicate<TSub> : IExistsPredicate
		where TSub : class
	{
		public IPredicate Predicate { get; set; }
		public bool Not { get; set; }

		public string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters)
		{
			var mapSub = GetClassMapper(typeof(TSub), sqlGenerator.Configuration);
			var sql = string.Format("({0}EXISTS (SELECT 1 FROM {1} WHERE {2}))",
				Not ? "NOT " : string.Empty,
				sqlGenerator.GetTableName(mapSub),
				Predicate.GetSql(sqlGenerator, parameters));
			return sql;
		}

		protected virtual IClassMapper GetClassMapper(Type type, IDapperExtensionsConfiguration configuration)
		{
			IClassMapper map = configuration.GetMap(type);
			if (map == null)
			{
				throw new NullReferenceException(string.Format("Map was not found for {0}", type));
			}

			return map;
		}
	}
}