using System.Collections.Generic;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public class PropertyPredicate<T, T2> : ComparePredicate, IPropertyPredicate
		where T : class
		where T2 : class
	{
		public string PropertyName2 { get; set; }

		public override string GetSql(ISqlGenerationContext context, IDictionary<string, object> parameters)
		{
			var classMapLeft = context.ClassMapperRepository.GetMapOrThrow<T>();
			var classMapRight = context.ClassMapperRepository.GetMapOrThrow<T2>();

			var columnName = classMapLeft.GetColumnName(context.Dialect, PropertyName);
			var columnName2 = classMapRight.GetColumnName(context.Dialect, PropertyName2);
			return $"({columnName} {GetOperatorString()} {columnName2})";
		}
	}
}