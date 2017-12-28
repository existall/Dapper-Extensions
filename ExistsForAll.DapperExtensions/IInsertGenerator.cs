﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	internal interface IInsertGenerator
	{
		ActionParams Insert(IClassMapper classMap, object entity);
		MultiActionParams InsertBatch(IClassMapper classMap, IEnumerable<object> entities);
	}

	internal class InsertGenerator : GeneratorBase,  IInsertGenerator
	{
		public InsertGenerator(IDapperExtensionsConfiguration configuration,
			IClassMapperRepository classMapperRepository,
			ISqlGenerator sqlGenerator) 
			: base(configuration, classMapperRepository, sqlGenerator)
		{
		}

		public ActionParams Insert(IClassMapper classMap, object entity)
		{
			var columns = classMap.GetNotIgnoredColumns();

			var guids = new IPropertyMap[0];

			if (Configuration.AutoPopulateKeyGuidValue)
			{
				guids = columns.Where(x => x.PropertyInfo.PropertyType == typeof(Guid)).ToArray();

				foreach (var guid in guids)
				{
					if ((Guid) guid.Getter(entity) != Guid.Empty)
						continue;

					var value = Configuration.GuidCreator.GetGuid();
					guid.Setter(entity, value);
				}
			}

			var sql = SqlGenerator.Insert(classMap);

			var dynamicParameters = new DynamicParameters();

			foreach (var column in columns)
			{
				dynamicParameters.Add(column.Name, column.Getter(entity));
			}

			if (classMap.HasAutoGeneratedId() && SqlGenerator.SupportsMultipleStatements())
			{
				sql += Configuration.Dialect.BatchSeparator + SqlGenerator.IdentitySql(classMap);
			}

			return ActionParams.New(sql, dynamicParameters);
		}

		public MultiActionParams InsertBatch(IClassMapper classMap, IEnumerable<object> entities)
		{
			var parameters = new List<DynamicParameters>();

			var columns = classMap.GetMutableColumns();

			var guids = new IPropertyMap[0];

			if (Configuration.AutoPopulateKeyGuidValue)
			{
				guids = columns.Where(x => x.PropertyInfo.PropertyType == typeof(Guid)).ToArray();
			}

			foreach (var e in entities)
			{
				foreach (var guid in guids)
				{
					if ((Guid)guid.Getter(e) == Guid.Empty)
					{
						var value = Configuration.GuidCreator.GetGuid();
						guid.Setter(e, value);
					}
				}

				var dynamicParameters = new DynamicParameters();

				foreach (var column in columns)
				{
					dynamicParameters.Add(column.Name, column.Getter(e));
				}

				parameters.Add(dynamicParameters);
			}

			var sql = SqlGenerator.Insert(classMap);

			return MultiActionParams.New(sql, parameters);
		}
	}
}