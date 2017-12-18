using System;
using System.Collections.Generic;
using System.Linq;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions.UnitTests.Sql
{
	internal class TestSqlDialect : ISqlDialect
	{
		public const string IdentitySql = "identitySql";
		public static Func<int, int, string> PageSql = (page, itemPerPage) => $"pageSql - {page} - {itemPerPage}";
		public static Func<int, int, string> SetSql = (firstResult, maxResult) => $"setSql - {firstResult} - {maxResult}";

		public char OpenQuote => '`';
		public char CloseQuote => '`';
		public string BatchSeparator => ";" + Environment.NewLine;
		public bool SupportsMultipleStatements => true;
		public char ParameterPrefix => '@';
		public string EmptyExpression => "1=1";

		public string GetTableName(string schemaName, string tableName, string alias)
		{
			return tableName;
		}

		public string GetColumnName(string prefix, string columnName, string alias)
		{
			return columnName;
		}

		public string GetIdentitySql(string tableName)
		{
			return IdentitySql;
		}

		public string GetPagingSql(string sql, int page, int resultsPerPage, IDictionary<string, object> parameters)
		{
			return $"{sql} {PageSql(page, resultsPerPage)}";
		}

		public string GetSetSql(string sql, int firstResult, int maxResults, IDictionary<string, object> parameters)
		{
			return $"{sql} {SetSql(firstResult, maxResults)}";
		}

		public bool IsQuoted(string value)
		{
			if (value.Trim()[0] == OpenQuote)
			{
				return value.Trim().Last() == CloseQuote;
			}

			return false;
		}

		public string QuoteString(string value)
		{
			if (IsQuoted(value) || value == "*")
			{
				return value;
			}
			return $"{OpenQuote}{value.Trim()}{CloseQuote}";
		}
	}
}