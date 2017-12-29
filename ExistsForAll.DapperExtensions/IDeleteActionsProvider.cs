using System.Collections.Generic;
using Dapper;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Predicates;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	internal interface IDeleteActionsProvider
	{
		ActionParams Delete(IClassMapper classMapper, IPredicate predicate);
	}

	internal class DeleteActionsProvider : ActionProviderBase, IDeleteActionsProvider
	{
		public DeleteActionsProvider(IDapperExtensionsConfiguration configuration,
			IClassMapperRepository classMapperRepository,
			ISqlGenerator sqlGenerator) 
			: base(configuration, classMapperRepository, sqlGenerator)
		{ }

		public ActionParams Delete(IClassMapper classMap, IPredicate predicate)
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Delete(classMap, predicate, parameters);
			var dynamicParameters = new DynamicParameters();

			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return ActionParams.New(sql, dynamicParameters);
		}
	}
}