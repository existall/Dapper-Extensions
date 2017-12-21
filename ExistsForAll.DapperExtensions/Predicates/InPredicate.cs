using System.Collections;
using System.Collections.Generic;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions.Predicates
{
	public class InPredicate<T> : BasePredicate, IInPredicate
		where T : class
	{
		public ICollection Collection { get; }
		public bool Not { get; set; }

		public InPredicate(ICollection collection, string propertyName, bool isNot = false)
		{
			PropertyName = propertyName;
			Collection = collection;
			Not = isNot;
		}

		private static string GetIsNotStatement(bool not)
		{
			return not ? "NOT " : string.Empty;
		}

		public override string GetSql(ISqlGenerationContext context, IDictionary<string, object> parameters)
		{
			var columnName = context.ClassMap.GetColumnName(context.Dialect, PropertyName);

			var @params = new List<string>();

			foreach (var item in Collection)
			{
				@params.Add(parameters.SetParameterName(PropertyName, item, context.Dialect.ParameterPrefix));
			}

			var commaDelimited = string.Join(",", @params);

			return $@"({columnName} {GetIsNotStatement(Not)} IN ({commaDelimited}))";
		}
	}
}