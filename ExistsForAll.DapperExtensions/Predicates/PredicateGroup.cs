using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExistsForAll.DapperExtensions.Predicates
{
	/// <summary>
	/// Groups IPredicates together using the specified group operator.
	/// </summary>
	public class PredicateGroup : IPredicateGroup
	{
		public GroupOperator Operator { get; set; }

		public IList<IPredicate> Predicates { get; set; }

		public string GetSql(ISqlGenerationContext context, IDictionary<string, object> parameters)
		{
			var seperator = Operator == GroupOperator.And ? " AND " : " OR ";
			return "(" + Predicates.Aggregate(new StringBuilder(),
					   (sb, p) => (sb.Length == 0 ? sb : sb.Append(seperator)).Append(p.GetSql(context, parameters)),
					   sb =>
					   {
						   var s = sb.ToString();
						   if (s.Length == 0) return context.Dialect.EmptyExpression;
						   return s;
					   }
				   ) + ")";
		}
	}
}