using System;
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
		Func<object,object> Getter { get; }
		void Setter(object entity, object value);
		PropertyInfo PropertyInfo { get; }
	}
}