using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions
{
	internal class ClassMapperRepository : IClassMapperRepository
	{
		private readonly ConcurrentDictionary<RuntimeTypeHandle, IClassMapper> _mapIndex = new ConcurrentDictionary<RuntimeTypeHandle, IClassMapper>();

		public void Initialize(IEnumerable<Assembly> assemblies, Type mapperType)
		{
			InitializeMapping(assemblies, mapperType);
		}

		public IClassMapper GetMap(Type entityType)
		{
			if (_mapIndex.TryGetValue(entityType.TypeHandle, out var map))
				return (IClassMapper)map;

			return null;
		}

		public IClassMapper GetMap<T>()
		{
			return GetMap(typeof(T));
		}

		public void ClearCache()
		{
			_mapIndex.Clear();
		}

		private void InitializeMapping(IEnumerable<Assembly> assemblies, Type mapperType)
		{
			var types = assemblies
				.SelectMany(x => x.GetTypes())
				.Where(x => typeof(IClassMapper<>).IsAssignableFrom(x));

			foreach (var type in types)
			{
				var local = type;
				var key = local.GetGenericArguments()[0].TypeHandle;
				_mapIndex.GetOrAdd(key, k => (IClassMapper)Activator.CreateInstance(local));
			}
		}
	}
}