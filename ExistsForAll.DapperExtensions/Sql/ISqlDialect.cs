using System.Collections.Generic;
using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.Sql
{
	public interface ISqlDialect
	{
		char OpenQuote { get; }
		char CloseQuote { get; }
		string BatchSeparator { get; }
		bool SupportsMultipleStatements { get; }
		char ParameterPrefix { get; }
		string EmptyExpression { get; }
		string GetTableName(string schemaName, string tableName, string alias);
		string GetColumnName(string prefix, string columnName, string alias);
		string GetIdentitySql(string tableName);
		string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters);
		string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters);
		bool IsQuoted(string value);
		string QuoteString(string value);
		string GetUpsertSql(IClassMapper classMapper);
	}
}