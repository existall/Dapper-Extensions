using System;
using System.Collections.Generic;
using System.Numerics;

namespace ExistsForAll.DapperExtensions.Mapper
{
	public abstract class ClassMapperBase
	{
		protected static Dictionary<Type, KeyType> PropertyTypeKeyTypeMapping { get; }

		static ClassMapperBase()
		{
			PropertyTypeKeyTypeMapping = new Dictionary<Type, KeyType>
			{
				{ typeof(byte), KeyType.Identity }, { typeof(byte?), KeyType.Identity },
				{ typeof(sbyte), KeyType.Identity }, { typeof(sbyte?), KeyType.Identity },
				{ typeof(short), KeyType.Identity }, { typeof(short?), KeyType.Identity },
				{ typeof(ushort), KeyType.Identity }, { typeof(ushort?), KeyType.Identity },
				{ typeof(int), KeyType.Identity }, { typeof(int?), KeyType.Identity },
				{ typeof(uint), KeyType.Identity}, { typeof(uint?), KeyType.Identity },
				{ typeof(long), KeyType.Identity }, { typeof(long?), KeyType.Identity },
				{ typeof(ulong), KeyType.Identity }, { typeof(ulong?), KeyType.Identity },
				{ typeof(BigInteger), KeyType.Identity }, { typeof(BigInteger?), KeyType.Identity },
				{ typeof(Guid), KeyType.Guid }, { typeof(Guid?), KeyType.Guid },
			};
		}
	}
}