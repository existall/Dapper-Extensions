using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public interface ISqlGenerationContext
	{
		ISqlDialect  Dialect { get; }
		IClassMapperRepository ClassMapperRepository { get; }
	}

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