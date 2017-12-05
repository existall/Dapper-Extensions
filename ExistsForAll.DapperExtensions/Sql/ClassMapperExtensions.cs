﻿using System;
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
			var propertyMap = map.Properties.SingleOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
			if (propertyMap == null)
			{
				throw new ArgumentException(string.Format("Could not find '{0}' in Mapping.", propertyName));
			}

			return GetColumnName(map, propertyMap, sqlDialect, includeAlias);
		}

		public static string BuildSelectColumns(this IClassMapper classMap, ISqlDialect sqlDialect)
		{
			var columns = classMap.Properties
				.Where(p => !p.Ignored)
				.Select(p => classMap.GetColumnName(p, sqlDialect, true));
			return columns.AppendStrings();
		}

		public static IPropertyMap[] GetMutableColumns(this IClassMapper classMapper)
		{
			return classMapper.Properties
				.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity || p.KeyType == KeyType.TriggerIdentity))
				.ToArray();
		}

		public static IPropertyMap[] GetTriggerIdentities(this IClassMapper classMapper)
		{
			return classMapper.Properties.Where(p => p.KeyType == KeyType.TriggerIdentity).ToArray();
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
			return classMapper.Properties.SingleOrDefault(p => p.Name == propertyName);
		}
	}
}