using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public interface ISqlGenerationContext
	{
		ISqlDialect  Dialect { get; }
		IClassMapperRepository ClassMapperRepository { get; }
	}
}