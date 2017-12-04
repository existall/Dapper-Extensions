using System.Collections.Generic;

namespace ExistsForAll.DapperExtensions
{
	public interface IMultipleResultReader
	{
		IEnumerable<T> Read<T>();
	}
}