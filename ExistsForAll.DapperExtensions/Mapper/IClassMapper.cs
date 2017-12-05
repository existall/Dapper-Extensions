using System;

namespace ExistsForAll.DapperExtensions.Mapper
{
	public interface IClassMapper
	{
		string SchemaName { get; }
		string TableName { get; }
		IPropertyMapCollection Properties { get; }
		Type EntityType { get; }
	}
}