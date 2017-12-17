﻿using System;
using System.Collections.Generic;
using System.Linq;
using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.Sql
{
	internal static class ClassMapperExtensions
	{
		public static string IdentitySql(this IClassMapper classMap, ISqlDialect sqlDialect)
		{
			return sqlDialect.GetIdentitySql(classMap.GetTableName(sqlDialect));
		}

		public static string GetTableName(this IClassMapper map, ISqlDialect sqlDialect)
		{
			return sqlDialect.GetTableName(map.SchemaName, map.TableName, null);
		}

		public static string GetColumnName(this IClassMapper map, IPropertyMap property, ISqlDialect sqlDialect, bool includeAlias)
		{
			string alias = null;
			if (property.ColumnName != property.Name && includeAlias)
			{
				alias = property.Name;
			}

			return sqlDialect.GetColumnName(map.GetTableName(sqlDialect), property.ColumnName, alias);
		}

		public static string GetColumnName(this IClassMapper map, ISqlDialect sqlDialect, string propertyName, bool includeAlias)
		{
			var propertyMap = map.Properties.GetByName(propertyName);
			
			return GetColumnName(map, propertyMap, sqlDialect, includeAlias);
		}

		public static string BuildSelectColumns(this IClassMapper classMap, ISqlDialect sqlDialect)
		{
			var columns = classMap.Properties
				.Where(p => !p.Ignored)
				.Select(p => classMap.GetColumnName(p, sqlDialect, true));
			return columns.AppendStrings();
		}

		public static string GetColumnName(this IClassMapper classMapper,ISqlDialect sqlDialect ,string propertyName)
		{
			var propertyMap = classMapper.GetPropertyMapByName(propertyName);

			if (propertyMap == null)
				throw new NullReferenceException($"{propertyName} was not found for {classMapper.EntityType}");
			
			return classMapper.GetColumnName(propertyMap, sqlDialect, false);
		}

		public static IPropertyMap GetPropertyMapByName(this IClassMapper classMapper, string propertyName)
		{
			return classMapper.Properties.GetByName(propertyName);
		}

		public static IPropertyMap GetKeyMapByName(this IClassMapper classMapper, string propertyName)
		{
			return classMapper.Keys.GetByName(propertyName);
		}

		public static bool HasAutoGeneratedId(this IClassMapper classMapper)
		{
			var count = classMapper.Keys.Count(x => x.Ignored);

			if(count > 1)
				throw new InvalidOperationException($"{classMapper.EntityType} has more than one auto generated id.");

			return count == 1;
		}

		public static IPropertyMap GetAutoGeneratedId(this IClassMapper classMapper)
		{
			return classMapper.Keys.SingleOrDefault(x => x.Ignored);
		}

		public static IPropertyMap[] GetMutableColumns(this IClassMapper classMapper)
		{
			return classMapper.Keys.Where(p => !p.Ignored && !p.IsReadOnly)
				.Concat(classMapper.Properties.Where(x => !x.Ignored && !x.IsReadOnly))
				.ToArray();
		}

		public static IPredicate GetIdPredicate(this IClassMapper classMap, object id)
		{
			var isSimpleType = id.GetType().IsSimpleType();

			var keys = classMap.Keys;

			IDictionary<string, object> paramValues = null;

			IList<IPredicate> predicates = new List<IPredicate>();

			if (!isSimpleType)
			{
				paramValues = XExtensions.GetObjectValues(id);
			}

			foreach (var key in keys)
			{
				var value = id;
				if (!isSimpleType)
				{
					value = paramValues[key.Name];
				}

				var predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);

				var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
				fieldPredicate.Not = false;
				fieldPredicate.Operator = Operator.Eq;
				fieldPredicate.PropertyName = key.Name;
				fieldPredicate.Value = value;
				predicates.Add(fieldPredicate);
			}

			return predicates.Count == 1
				? predicates[0]
				: new PredicateGroup
				{
					Operator = GroupOperator.And,
					Predicates = predicates
				};
		}
	}
}