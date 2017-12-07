using System.Reflection;

namespace ExistsForAll.DapperExtensions.Mapper
{
	/// <summary>
	/// Maps an entity property to its corresponding column in the database.
	/// </summary>
	public interface IPropertyMap
	{
		string Name { get; }
		string ColumnName { get; }
		bool Ignored { get; }
		bool IsReadOnly { get; }
		PropertyInfo PropertyInfo { get; }
	}
}