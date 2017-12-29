using System;
using System.Collections.Generic;
using Dapper;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Predicates;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	internal interface IAtomicIncrementActionProvider
	{
		ActionParams AtomicIncrement(IClassMapper classMap, IPredicate predicate, IProjection projection, int amount);
	}

	internal class AtomicIncrementActionProvider : ActionProviderBase, IAtomicIncrementActionProvider
	{
		public AtomicIncrementActionProvider(IDapperExtensionsConfiguration configuration,
			IClassMapperRepository classMapperRepository,
			ISqlGenerator sqlGenerator) : base(configuration, classMapperRepository, sqlGenerator)
		{
		}

		public ActionParams AtomicIncrement(IClassMapper classMap, IPredicate predicate, IProjection projection, int amount)
		{
			var target = classMap.GetPropertyMapByName(projection.PropertyName);

			if (target.Ignored || target.IsReadOnly)
				throw new InvalidOperationException(
					$"Atomic increment is not allowed on {projection.PropertyName} for type {classMap.EntityType}. It's either ignored or read only");

			var wherePredicate = classMap.GetPredicate(predicate);
			var parameters = new Dictionary<string, object>();

			var sql = SqlGenerator.AtomicIncrement(classMap, wherePredicate, parameters, projection, amount);

			var dynamicParameters = new DynamicParameters();

			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return ActionParams.New(sql, dynamicParameters);
		}
	}
}