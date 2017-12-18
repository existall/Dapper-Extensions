using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public class FieldPredicate<T> : ComparePredicate, IFieldPredicate
		where T : class
	{
		public object Value { get; set; }

		public override string GetSql(ISqlGenerationContext context, IDictionary<string, object> parameters)
		{
			var classMap = context.ClassMap;

			var columnName = classMap.GetColumnName(context.Dialect, PropertyName);

			if (Value == null)
				return GetSqlForNull(columnName);

			if (Value is IEnumerable && !(Value is string))
				return GetSqlForEnumerable(columnName, parameters, context.Dialect);

			return GetSqlFromSingleStringField(columnName, parameters, context.Dialect);
		}

		private string GetSqlForNull(string columnName)
		{
			var isNotString = Not ? "NOT " : string.Empty;
			return $"({columnName} IS {isNotString}NULL)";
		}

		private string GetSqlForEnumerable(string columnName, IDictionary<string, object> parameters, ISqlDialect sqlDialect)
		{
			if (Operator != Operator.Eq)
				throw new ArgumentException("Operator must be set to Eq for Enumerable types");

			var @params = new List<string>();

			foreach (var value in (IEnumerable)Value)
			{
				var valueParameterName = parameters.SetParameterName(PropertyName, value, sqlDialect.ParameterPrefix);
				@params.Add(valueParameterName);
			}

			var paramStrings = @params.Aggregate(new StringBuilder(), (sb, s) => sb.Append((sb.Length != 0 ? ", " : string.Empty) + s), sb => sb.ToString());

			var notIn = Not ? "NOT " : string.Empty;

			return $"({columnName} {notIn}IN ({paramStrings}))";
		}

		private string GetSqlFromSingleStringField(string columnName, IDictionary<string, object> parameters, ISqlDialect sqlDialect)
		{
			var parameterName = parameters.SetParameterName(PropertyName, Value, sqlDialect.ParameterPrefix);
			return $"({columnName} {GetOperatorString()} {parameterName})";
		}
	}
}