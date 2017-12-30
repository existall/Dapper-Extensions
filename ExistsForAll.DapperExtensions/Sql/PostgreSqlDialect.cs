using System.Collections.Generic;
using System.Linq;
using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.Sql
{
	public class PostgreSqlDialect : SqlDialectBase
	{
		public override string GetIdentitySql(string tableName)
		{
			return "SELECT LASTVAL() AS Id";
		}

		public override string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
		{
			var startValue = page * resultsPerPage;
			return GetSetSql(sql, startValue, resultsPerPage, parameters);
		}

		public override string GetSetSql(string sql, int pageNumber, int maxResults, IDictionary<string, object> parameters)
		{
			var result = string.Format("{0} LIMIT @maxResults OFFSET @pageStartRowNbr", sql);
			parameters.Add("@maxResults", maxResults);
			parameters.Add("@pageStartRowNbr", pageNumber * maxResults);
			return result;
		}

		public override string GetColumnName(string prefix, string columnName, string alias)
		{
			return GetColumnName(columnName, alias);
		}

		public override string GetTableName(string schemaName, string tableName, string alias)
		{
			return base.GetTableName(schemaName, tableName, alias).ToLower();
		}

		public override string GetUpsertSql(IClassMapper classMapper)
		{
			return
				$"INSERT INTO {GetTableName(classMapper.SchemaName, classMapper.TableName, null)} {GetInsertSection(classMapper)} ON CONFLICT ({GetKeysConstraint(classMapper)}) DO UPDATE SET {GetUpdateSection(classMapper)};";

		}

		private string GetColumnName(string columnName, string alias)
		{
			return base.GetColumnName(null, columnName, alias).ToLower();
		}

		private string GetInsertSection(IClassMapper classMap)
		{
			var columns = classMap.GetNotIgnoredColumns();

			var columnNames = columns.Select(p => GetColumnName(p.ColumnName, null));

			var parameters = columns.Select(p => ParameterPrefix + p.Name);

			return $"({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", parameters)})";
		}

		private string GetUpdateSection(IClassMapper classMap)
		{
			var columns = classMap.GetMutableColumns();

			var updateSql = columns.Select(
				p => $"{GetColumnName(p.ColumnName, null)} = {ParameterPrefix}{p.Name}");

			return string.Join(", ", updateSql);
		}

		private string GetKeysConstraint(IClassMapper classMap)
		{
			var keysName = classMap.Keys.Select(x => GetColumnName(x.ColumnName, null));

			return string.Join(", ", keysName);
		}
	}

}
