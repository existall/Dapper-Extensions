using System.Collections.Generic;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions.Predicates
{
	public class PropertyPredicate : ComparePredicate, IPropertyPredicate
	{
		public string PropertyName2 { get; set; }

		public override string GetSql(ISqlGenerationContext context, IDictionary<string, object> parameters)
		{
			var classMap = context.ClassMap;

			var columnName = classMap.GetColumnName(context.Dialect, PropertyName);
			var columnName2 = classMap.GetColumnName(context.Dialect, PropertyName2);
			return $"({columnName} {GetOperatorString()} {columnName2})";
		}
	}
}