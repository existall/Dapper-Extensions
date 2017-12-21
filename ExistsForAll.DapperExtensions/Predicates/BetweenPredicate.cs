using System.Collections.Generic;
using System.Text;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions.Predicates
{
	public class BetweenPredicate<T> : BasePredicate, IBetweenPredicate
		where T : class
	{
		public BetweenValues Value { get; set; }

		public bool Not { get; set; }

		public override string GetSql(ISqlGenerationContext context, IDictionary<string, object> parameters)
		{
			var classMap = context.ClassMap;

			var columnName = classMap.GetColumnName(context.Dialect, PropertyName);

			var propertyName1 = parameters.SetParameterName(PropertyName, Value.Value1, context.Dialect.ParameterPrefix);

			var propertyName2 = parameters.SetParameterName(PropertyName, Value.Value2, context.Dialect.ParameterPrefix);

			var sb = new StringBuilder();

			sb.Append(columnName)
				.Append(" ")
				.Append(Not ? "NOT " : string.Empty)
				.Append("BETWEEN ")
				.Append(propertyName1)
				.Append(" AND ")
				.Append(propertyName2);

			return sb.ToString();
		}
	}
}