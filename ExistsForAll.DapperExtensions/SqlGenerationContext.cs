using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	internal struct SqlGenerationContext : ISqlGenerationContext
	{
		public ISqlDialect Dialect { get; }
		public IClassMapper ClassMap { get; }

		public SqlGenerationContext(ISqlDialect dialect, IClassMapper classMap)
		{
			Dialect = dialect;
			ClassMap = classMap;
		}
	}
}