using System;
using System.Linq;
using Xunit;

namespace ExistsForAll.DapperExtensions.UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
	        var map = new Mapper();

	        var propertyMap = map.Keys.First();

			var foo = new Foo();

			propertyMap.Setter(foo, "foo");

	        var propertyMapGetter = propertyMap.Getter(foo);
        }
    }
}
