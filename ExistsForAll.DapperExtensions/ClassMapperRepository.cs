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
			return _mapIndex.TryGetValue(entityType.TypeHandle, out var map) ? map : throw new ClassMapException($"No ClassMap was found for type {entityType}.");
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
			var types = assemblies.SelectMany(x => x.GetTypes()).ToArray();

			var fullName = typeof(IClassMapper<>).FullName;

			var enumerable = types
				.Select(type => new {Type = type, Interface = type.GetTypeInfo().GetInterface(fullName)})
				.Where(t => t.Interface != null)
				.ToArray();

			//var types = assemblies
			//	.SelectMany(x => x.GetTypes())
			//	.Where(x => typeof(IClassMapper).IsAssignableFrom(x));

			foreach (var type in enumerable)
			{
				var local = type;
				var key = local.Interface.GetGenericArguments()[0].TypeHandle;
				_mapIndex.GetOrAdd(key, k => (IClassMapper)Activator.CreateInstance(local.Type));
			}
		}
	}
}