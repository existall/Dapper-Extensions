using System.Collections.Generic;

namespace ExistsForAll.DapperExtensions
{
	public abstract class BasePredicate : IBasePredicate
	{
		public string PropertyName { get; set; }

		public abstract string GetSql(ISqlGenerationContext context, IDictionary<string, object> parameters);
	}
}