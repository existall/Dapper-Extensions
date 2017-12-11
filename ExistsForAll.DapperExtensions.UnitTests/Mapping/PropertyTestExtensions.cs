using ExistsForAll.DapperExtensions.Mapper;
using Xunit;

namespace ExistsForAll.DapperExtensions.UnitTests.Mapping
{
	internal static class PropertyTestExtensions
	{
		public static void AssertPropertyMap(this IPropertyMap sut, string columnName, bool isIgnored = false, bool isReadOnly = false)
		{
			Assert.Equal(sut.ColumnName, columnName);
			Assert.Equal(sut.Ignored, isIgnored);
			Assert.Equal(sut.IsReadOnly, isReadOnly);
		}
	}
}