using System;
using System.Linq.Expressions;
using ExistsForAll.DapperExtensions.Mapper;
using Xunit;

namespace ExistsForAll.DapperExtensions.UnitTests.Mapping
{
	public class PropertyMapTests
	{
		private const string IdColumnName = nameof(IntEntity.Id);

		[Fact]
		public void PropertyMap_WhenCreatingMapWithDefaultValue_ShouldValuesBeDefault()
		{
			var sut = BuildMap<IntEntity, int>(x => x.Id);

			sut.AssertPropertyMap(IdColumnName);
		}

		[Fact]
		public void PropertyMap_WhenSettingColumnName_ShouldReturnValidName()
		{
			const string columnName = "new_name";

			var sut = BuildMap<IntEntity, int>(x => x.Id);

			sut.Column(columnName);

			sut.AssertPropertyMap(columnName);
		}

		[Fact]
		public void PropertyMap_WhenIsReadOnly_ShouldReturnTrue()
		{
			var sut = BuildMap<IntEntity, int>(x => x.Id);

			sut.ReadOnly();

			sut.AssertPropertyMap(IdColumnName, isReadOnly: true);
		}

		[Fact]
		public void PropertyMap_WhenIsIgnoredOnly_ShouldReturnTrue()
		{
			var sut = BuildMap<IntEntity, int>(x => x.Id);

			sut.Ignore();

			sut.AssertPropertyMap(IdColumnName, isIgnored:true);
		}

		[Fact]
		public void PropertyMap_WhenUsingSetter_ShouldSetTheValueInTheEntityProperty()
		{
			const int newId = 12345;

			var sut = BuildMap<IntEntity, int>(x => x.Id);

			var entity = new IntEntity();

			sut.Setter(entity, newId);

			sut.AssertPropertyMap(IdColumnName);

			Assert.Equal(entity.Id, newId);
		}

		[Fact]
		public void PropertyMap_WhenUsingGetter_ShouldGetterTheCurrentValueFromTheEntityProperty()
		{
			const int newId = 54321;

			var sut = BuildMap<IntEntity, int>(x => x.Id);

			var entity = new IntEntity {Id = newId};

			var result = sut.Getter(entity);

			sut.AssertPropertyMap(IdColumnName);

			Assert.Equal(result, newId);
		}

		private PropertyMap BuildMap<T, TOut>(Expression<Func<T, TOut>> expression)
		{
			return (PropertyMap) PropertyMapBuilder.BuildMap(expression);
		}
	}
}
