using System.Collections;
using System.Collections.Generic;

namespace ExistsForAll.DapperExtensions.Mapper
{
	internal class PropertyMapCollection : IPropertyMapCollection
	{
		private readonly Dictionary<string, IPropertyMap> _index = new Dictionary<string, IPropertyMap>();

		public void Add(IPropertyMap propertyMap)
		{
			Guard.ArgumentNull(propertyMap, nameof(PropertyMap));
			_index.Add(propertyMap.Name, propertyMap);
		}

		public void Remove(IPropertyMap propertyMap)
		{
			_index.Remove(propertyMap.Name);
		}

		public void Remove(string name)
		{
			Guard.ArgumentNull(name, nameof(name));

			if (!_index.ContainsKey(name))
				throw new PropertyMapNotFoundException(name);

			_index.Remove(name);
		}

		public IPropertyMap GetByName(string name, bool throwOnMissing)
		{
			Guard.ArgumentNull(name, nameof(name));

			if (!_index.TryGetValue(name, out var propertyMap) && throwOnMissing)
				throw new PropertyMapNotFoundException(name);
			
			return propertyMap;
		}

		public IEnumerable<string> Names => _index.Keys;

		public IEnumerator<IPropertyMap> GetEnumerator()
		{
			return _index.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _index.Values.GetEnumerator();
		}
	}
}