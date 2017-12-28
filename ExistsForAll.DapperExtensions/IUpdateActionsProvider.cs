using System.Collections.Generic;
using System.Linq;
using Dapper;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Predicates;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	internal interface IUpdateActionsProvider
	{
		ActionParams Update(IClassMapper classMap, IPredicate predicate, IList<IProjectionSet> projectionSets);
		ActionParams Update(IClassMapper classMap, object entity);
	}

	internal class UpdateActionsProvider : GeneratorBase, IUpdateActionsProvider
	{
		public UpdateActionsProvider(IDapperExtensionsConfiguration configuration,
			IClassMapperRepository classMapperRepository,
			ISqlGenerator sqlGenerator)
			: base(configuration, classMapperRepository, sqlGenerator)
		{
		}

		public ActionParams Update(IClassMapper classMap, IPredicate predicate, IList<IProjectionSet> projectionSets)
		{
			var parameters = new Dictionary<string, object>();
			var dynamicParameters = new DynamicParameters();

			var sql = SqlGenerator.Update(classMap, predicate, projectionSets, parameters);

			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return ActionParams.New(sql, dynamicParameters);
		}

		public ActionParams Update(IClassMapper classMap, object entity)
		{
			var predicate = classMap.GetKeyPredicate(entity);
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.Update(classMap, predicate, parameters);
			var dynamicParameters = new DynamicParameters();

			var columns = classMap.GetMutableColumns();

			foreach (var property in ReflectionHelper.GetObjectValues(entity)
				.Where(property => columns.Any(c => c.Name == property.Key)))
			{
				dynamicParameters.Add(property.Key, property.Value);
			}

			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return ActionParams.New(sql, dynamicParameters);
		}
	}
}