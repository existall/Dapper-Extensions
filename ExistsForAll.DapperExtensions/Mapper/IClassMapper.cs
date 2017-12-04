using System;
using System.Collections.Generic;

namespace ExistsForAll.DapperExtensions.Mapper
{
	public interface IClassMapper
	{
		string SchemaName { get; }
		string TableName { get; }
		IList<IPropertyMap> Properties { get; }
		Type EntityType { get; }
	}
}