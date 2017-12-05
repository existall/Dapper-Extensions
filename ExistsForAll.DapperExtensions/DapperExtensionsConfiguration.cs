using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Sql;

namespace ExistsForAll.DapperExtensions
{
	public class DapperExtensionsConfiguration : IDapperExtensionsConfiguration
	{
		public DapperExtensionsConfiguration()
			: this(typeof(AutoClassMapper<>), new List<Assembly>(), new SqlServerDialect())
		{
		}

		public DapperExtensionsConfiguration(Type defaultMapper, IList<Assembly> mappingAssemblies, ISqlDialect sqlDialect)
		{
			DefaultMapper = defaultMapper;
			MappingAssemblies = mappingAssemblies ?? new List<Assembly>();
			Dialect = sqlDialect;
		}

		public Type DefaultMapper { get; }
		public IList<Assembly> MappingAssemblies { get; }
		public ISqlDialect Dialect { get; }

		public Guid GetNextGuid()
		{
			byte[] b = Guid.NewGuid().ToByteArray();
			DateTime dateTime = new DateTime(1900, 1, 1);
			DateTime now = DateTime.Now;
			TimeSpan timeSpan = new TimeSpan(now.Ticks - dateTime.Ticks);
			TimeSpan timeOfDay = now.TimeOfDay;
			byte[] bytes1 = BitConverter.GetBytes(timeSpan.Days);
			byte[] bytes2 = BitConverter.GetBytes((long)(timeOfDay.TotalMilliseconds / 3.333333));
			Array.Reverse(bytes1);
			Array.Reverse(bytes2);
			Array.Copy(bytes1, bytes1.Length - 2, b, b.Length - 6, 2);
			Array.Copy(bytes2, bytes2.Length - 4, b, b.Length - 4, 4);
			return new Guid(b);
		}
	}
}