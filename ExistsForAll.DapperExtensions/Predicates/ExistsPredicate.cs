using System.Collections.Generic;
using System.Text;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions.Predicates
{
	public class ExistsPredicate<TSub> : IExistsPredicate
		where TSub : class
	{
		public IPredicate Predicate { get; set; }

		public bool Not { get; set; }

		public string GetSql(ISqlGenerationContext context, IDictionary<string, object> parameters)
		{
			var classMap = context.ClassMap;

			var sql = new StringBuilder();

			sql.Append(Not ? "NOT " : string.Empty)
				.Append("EXISTS (SELECT 1 FROM ")
				.Append(classMap.GetTableName(context.Dialect))
				.Append(" WHERE ")
				.Append(Predicate.GetSql(context, parameters))
				.Append("))");

			return sql.ToString();
		}
	}
}