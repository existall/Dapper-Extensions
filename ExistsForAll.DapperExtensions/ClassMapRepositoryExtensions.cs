using System;
using ExistsForAll.DapperExtensions.Mapper;

namespace ExistsForAll.DapperExtensions
{
	internal static class ClassMapRepositoryExtensions
	{
		public static IClassMapper GetMapOrThrow<T>(this IClassMapperRepository target)
		{
			return target.GetMapOrThrow(typeof(T));
		}

		public static IClassMapper GetMapOrThrow(this IClassMapperRepository target, Type entityType)
		{
			var classMap = target.GetMap(entityType);

			if (classMap == null)
				throw new NullReferenceException($"Map was not found for {entityType}");

			return classMap;
		}
	}
}