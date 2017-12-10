using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	internal struct SqlGenerationContext : ISqlGenerationContext
	{
		public ISqlDialect Dialect { get; }
		public IClassMapperRepository ClassMapperRepository { get; }

		public SqlGenerationContext(ISqlDialect dialect, IClassMapperRepository classMapperRepository)
		{
			Dialect = dialect;
			ClassMapperRepository = classMapperRepository;
		}
	}
}