using System;
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
		ActionParams Upsert(IClassMapper classMap, object entity);
	}

	internal class UpdateActionsProvider : ActionProviderBase, IUpdateActionsProvider
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

		public ActionParams Upsert(IClassMapper classMap, object entity)
		{
			var columns = classMap.GetNotIgnoredColumns();

			var guids = new IPropertyMap[0];

			if (Configuration.AutoPopulateKeyGuidValue)
			{
				guids = columns.Where(x => x.PropertyInfo.PropertyType == typeof(Guid)).ToArray();

				foreach (var guid in guids)
				{
					if ((Guid)guid.Getter(entity) != Guid.Empty)
						continue;

					var value = Configuration.GuidCreator.GetGuid();
					guid.Setter(entity, value);
				}
			}

			var sql = Configuration.Dialect.GetUpsertSql(classMap);

			var dynamicParameters = new DynamicParameters();

			foreach (var column in columns)
			{
				dynamicParameters.Add(column.Name, column.Getter(entity));
			}

			return ActionParams.New(sql, dynamicParameters);
		}
	}
}