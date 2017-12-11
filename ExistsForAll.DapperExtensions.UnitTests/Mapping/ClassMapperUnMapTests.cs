using System;
using Xunit;

namespace ExistsForAll.DapperExtensions.UnitTests.Mapping
{
	public partial class ClassMapperTests
	{
		private class IntEntityClassMapper : TestClassMapperBase<IntEntity>
		{
		}

		[Fact]
		public void UnMap_WhenRemovesAnExistingMapping_ShouldRemoveMappingFromProperties()
		{
			var target = new IntEntityClassMapper();

			target.Map(p => p.String);
			Assert.True(target.MappingExists(x => x.String));

			target.UnMap(p => p.String);
			Assert.False(target.MappingExists(x => x.String));
		}

		[Fact]
		public void UnMap_WhenUnMapNonExistenceProperty_ShouldThrowException()
		{
			var target = new IntEntityClassMapper();

			Assert.Throws<InvalidOperationException>(() => target.UnMap(p => p.String));
		}
	}
}
