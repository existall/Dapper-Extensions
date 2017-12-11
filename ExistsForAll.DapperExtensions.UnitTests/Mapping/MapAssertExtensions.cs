using ExistsForAll.DapperExtensions.Mapper;
using Xunit;

namespace ExistsForAll.DapperExtensions.UnitTests.Mapping
{
	internal static class MapAssertExtensions
	{
		public static void AssertColumnName(this IPropertyMap target, string columnName)
		{
			Assert.Equal(target.ColumnName, columnName);
		}

	}
}