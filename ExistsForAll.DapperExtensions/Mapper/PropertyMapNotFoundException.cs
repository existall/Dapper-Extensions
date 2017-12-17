using System;

namespace ExistsForAll.DapperExtensions.Mapper
{
	internal class PropertyMapNotFoundException : Exception
	{
		public PropertyMapNotFoundException(string propertyName) 
			: this(propertyName, null)
		{
		}

		public PropertyMapNotFoundException(string propertyName, Exception inner) 
			: base(GetMessage(propertyName), inner)
		{
		}

		private static string GetMessage(string propertyName)
		{
			return $"No mapping was found for {propertyName}";
		}
	}
}