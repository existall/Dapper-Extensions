using System.Collections.Generic;
using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions.Sql
{
	public interface ISqlGenerator
	{
		string Select<T>(IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters) where T : IClassMapper;
		string SelectPaged<T>(IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDictionary<string, object> parameters) where T : IClassMapper;
		string SelectSet<T>(IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDictionary<string, object> parameters) where T : IClassMapper;
		string Count<T>(IPredicate predicate, IDictionary<string, object> parameters) where T : IClassMapper;

		string Insert<T>() where T : IClassMapper;
		string Update<T>(IPredicate predicate, IDictionary<string, object> parameters, bool ignoreAllKeyProperties) where T : IClassMapper;
		string Delete<T>(IPredicate predicate, IDictionary<string, object> parameters) where T : IClassMapper;

		string IdentitySql<T>();
		//string GetTableName(IClassMapper map);
		//string GetColumnName(IClassMapper map, IPropertyMap property, bool includeAlias);
		//string GetColumnName(IClassMapper map, string propertyName, bool includeAlias);
		bool SupportsMultipleStatements();
	}
}