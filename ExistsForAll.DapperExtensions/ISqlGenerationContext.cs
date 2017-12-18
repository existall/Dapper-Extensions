using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public interface ISqlGenerationContext
	{
		ISqlDialect  Dialect { get; }
		IClassMapper ClassMap { get; }
	}
}