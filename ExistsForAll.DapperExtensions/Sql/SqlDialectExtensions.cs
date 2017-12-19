namespace ExistsForAll.DapperExtensions.Sql
{
	internal static class SqlDialectExtensions
	{
		public static string WrapWithQuotes(this ISqlDialect target, string phrase)
		{
			return $"{target.OpenQuote}{phrase}{target.CloseQuote}";
		}
	}
}
