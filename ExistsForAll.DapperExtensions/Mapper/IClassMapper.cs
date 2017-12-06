using System;

namespace ExistsForAll.DapperExtensions.Mapper
{
	public interface IClassMapper<T> : IClassMapper where T : class
	{
	}

	public interface IClassMapper
	{
		string SchemaName { get; }
		string TableName { get; }
		IPropertyMapCollection Properties { get; }
		IPropertyMapCollection Keys { get; }
		Type EntityType { get; }
	}
}