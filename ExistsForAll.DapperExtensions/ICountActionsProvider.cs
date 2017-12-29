using System.Collections.Generic;
using Dapper;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Predicates;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	internal interface ICountActionsProvider
	{
		ActionParams Count(IClassMapper classMap, IPredicate predicate);
	}

	internal class CountActionsProvider : ActionProviderBase, ICountActionsProvider
	{
		public CountActionsProvider(IDapperExtensionsConfiguration configuration,
			IClassMapperRepository classMapperRepository,
			ISqlGenerator sqlGenerator)
			: base(configuration, classMapperRepository, sqlGenerator)
		{
		}

		public ActionParams Count(IClassMapper classMap, IPredicate predicate)
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Count(classMap, predicate, parameters);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}
			return ActionParams.New(sql, dynamicParameters);
		}
	}
}