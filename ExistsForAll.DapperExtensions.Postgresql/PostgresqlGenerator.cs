﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Schema;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions.Postgresql
{
	public class DapperExtensionsBuilder
	{
		public DapperInstances BuildImplementor(IEnumerable<Assembly> assemblies, IDapperExtensionsConfiguration configuration)
		{
			// verify configuration;

			var repository = new ClassMapperRepository();
			repository.Initialize(assemblies, configuration.DefaultMapper);
			var sqlGenerator = new SqlGenerator(configuration);

			var dapperImplementor = new DapperImplementor(sqlGenerator, repository, configuration);
			var dapperAsyncImplementor = new DapperAsyncImplementor(sqlGenerator, repository, configuration);
			return new DapperInstances(dapperImplementor, dapperAsyncImplementor);
		}
	}
	
	public class PostgresqlGenerator : SqlGenerator, IPostgresqlGenerator
	{
		public PostgresqlGenerator(IDapperExtensionsConfiguration configuration)
			: base(configuration)
		{
		}

		public string Upsert(IClassMapper classMapper)
		{
			if (classMapper.HasAutoGeneratedId())
				throw new NotSupportedException($"Upsert does not support auto generated id on tyep [{classMapper.EntityType}].");

			return
				$"INSERT INTO {GetTableName(classMapper)} {GetInsertSection(classMapper)} ON CONFLICT ({GetKeysConstraint(classMapper)}) DO UPDATE SET {GetUpdateSection(classMapper)};";
		}

		private string GetInsertSection(IClassMapper classMap)
		{
			var columns = classMap.GetNotIgnoredColumns();

			var columnNames = columns.Select(p => GetColumnName(classMap, p, false));

			var parameters = columns.Select(p => Configuration.Dialect.ParameterPrefix + p.Name);

			return $"({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", parameters)})";
		}

		private string GetUpdateSection(IClassMapper classMap)
		{
			var columns = classMap.GetMutableColumns();

			var columnNames = columns.Select(p => GetColumnName(classMap, p, false));

			var parameters = columns.Select(p => Configuration.Dialect.ParameterPrefix + p.Name);

			var updateSql = columns.Select(
				p => $"{GetColumnName(classMap, p, false)} = {Configuration.Dialect.ParameterPrefix}{p.Name}");

			return string.Join(", ", updateSql);
		}

		private string GetKeysConstraint(IClassMapper classMap)
		{
			var keysName = classMap.Keys.Select(x => GetColumnName(classMap, x, false));

			return string.Join(", ", keysName);
		}
	}
}