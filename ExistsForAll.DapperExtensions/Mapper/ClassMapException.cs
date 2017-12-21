using System;

namespace ExistsForAll.DapperExtensions.Mapper
{
	public class ClassMapException : Exception
	{
	
		public ClassMapException(string message) : base(message)
		{
		}

		public ClassMapException(string message, Exception inner) 
			: base(message, inner)
		{
		}
	}
}