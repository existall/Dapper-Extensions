using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	internal abstract class GeneratorBase
	{
		protected IDapperExtensionsConfiguration Configuration { get; }
		protected IClassMapperRepository ClassMappers { get; }
		protected ISqlGenerator SqlGenerator { get; }
		
		public GeneratorBase(IDapperExtensionsConfiguration configuration,
			IClassMapperRepository classMapperRepository,
			ISqlGenerator sqlGenerator)
		{
			Configuration = configuration;
			ClassMappers = classMapperRepository;
			SqlGenerator = sqlGenerator;
		}
	}
}