using System.Collections.Generic;
using Dapper;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Predicates;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	internal class GetActionProvider : ActionProviderBase, IGetGenerator
	{
		public GetActionProvider(IDapperExtensionsConfiguration configuration, IClassMapperRepository classMapperRepository,
			ISqlGenerator sqlGenerator) : base(configuration, classMapperRepository, sqlGenerator)
		{
		}

		public ActionParams Get<T>(IClassMapper classMapper, object id)
		{
			var classMap = ClassMappers.GetMap<T>();
			var predicate = classMap.GetIdPredicate(id);
			var result = GetList(classMap, predicate, null, null);
			return result;
		}

		public ActionParams GetList(
			IClassMapper classMap,
			IPredicate predicate,
			IList<ISort> sort,
			IList<IProjection> projections)
		{
			var parameters = new Dictionary<string, object>();

			var sql = SqlGenerator.Select(classMap, predicate, sort, parameters, projections);

			var dynamicParameters = new DynamicParameters();

			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return ActionParams.New(sql, dynamicParameters);
		}

		public ActionParams GetPage(IClassMapper classMap,
			IPredicate predicate,
			IList<ISort> sort,
			IList<IProjection> projections, int page,
			int resultsPerPage)
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.SelectPaged(classMap, predicate, sort, page, resultsPerPage, parameters);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return ActionParams.New(sql, dynamicParameters);
		}

		public ActionParams GetSet(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult,
			int maxResults,
			IList<IProjection> projections)
		{
			var parameters = new Dictionary<string, object>();
			var sql = SqlGenerator.SelectSet(classMap, predicate, sort, firstResult, maxResults, parameters, projections);
			var dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return ActionParams.New(sql, dynamicParameters);
		}
	}
}