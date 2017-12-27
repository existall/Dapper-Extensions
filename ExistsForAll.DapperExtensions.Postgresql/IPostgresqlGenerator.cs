using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions.Postgresql
{
	public interface IPostgresqlGenerator : ISqlGenerator
	{
		string Upsert(IClassMapper classMapper);
	}
}