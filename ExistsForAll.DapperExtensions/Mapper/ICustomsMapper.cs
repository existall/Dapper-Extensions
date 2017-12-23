using System;

namespace ExistsForAll.DapperExtensions.Mapper
{
	public interface ICustomsMapper
	{
		object FromDatabase(Type destinationType, object input);
		object IntoDatabase(object input);
	}
}