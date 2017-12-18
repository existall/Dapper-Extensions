using System.Collections.Generic;

namespace ExistsForAll.DapperExtensions.Mapper
{
	public interface IPropertyMapCollection : IEnumerable<IPropertyMap>
	{
		void Add(IPropertyMap propertyMap);
		void Remove(IPropertyMap propertyMap);
		void Remove(string name);
		IPropertyMap GetByName(string name, bool throwOnMissing = true);
		IEnumerable<string> Names { get; }
	}
}