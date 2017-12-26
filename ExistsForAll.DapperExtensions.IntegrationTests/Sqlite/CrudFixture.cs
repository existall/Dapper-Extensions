using System;
using System.Collections.Generic;
using System.Linq;
using ExistsForAll.DapperExtensions.IntegrationTests.Data;
using ExistsForAll.DapperExtensions.Predicates;
using NUnit.Framework;

namespace ExistsForAll.DapperExtensions.IntegrationTests.Sqlite
{
	[TestFixture]
	public class CrudFixture
	{
		[TestFixture]
		public class InsertMethod : SqliteBaseFixture
		{
			[Test]
			public void AddsEntityToDatabase_ReturnsKey()
			{
				Person p = new Person
				{
					Active = true,
					FirstName = "Foo",
					LastName = "Bar",
					DateCreated = DateTime.UtcNow,
					Sex = Sex.Female
				};
				Db.Insert(p);
				var person = Db.Get<Person>(1);
				Assert.AreEqual(1, p.Id);
			}

			//[Test]
			//[Ignore("djskj")] //TODO: multikey identity
			//public void AddsEntityToDatabase_ReturnsCompositeKey()
			//{
			//    Multikey m = new Multikey { Key2 = "key", Value = "foo" };
			//    var key = Db.Insert(m);
			//    Assert.AreEqual(1, key.Key1);
			//    Assert.AreEqual("key", key.Key2);
			//}

			[Test]
			public void AddsEntityToDatabase_ReturnsGeneratedPrimaryKey()
			{
				Animal a1 = new Animal { Name = "Foo" };

				Db.Insert(a1);

				var a2 = Db.Get<Animal>(a1.Id);
				Assert.AreNotEqual(Guid.Empty, a2.Id);
				Assert.AreEqual(a1.Id, a2.Id);
			}

			[Test]
			public void AddsEntityToDatabase_WithPassedInGuid()
			{
				var guid = Guid.NewGuid();
				Animal a1 = new Animal { Id = guid, Name = "Foo" };
				Db.Insert(a1);

				var a2 = Db.Get<Animal>(a1.Id);
				Assert.AreNotEqual(Guid.Empty, a2.Id);
				Assert.AreEqual(guid, a2.Id);
			}

			[Test]
			public void AddsMultipleEntitiesToDatabase()
			{
				Animal a1 = new Animal { Name = "Foo" };
				Animal a2 = new Animal { Name = "Bar" };
				Animal a3 = new Animal { Name = "Baz" };

				Db.Insert<Animal>(new[] { a1, a2, a3 });

				var animals = Db.GetList<Animal>().ToList();
				Assert.AreEqual(3, animals.Count);
			}

			[Test]
			public void AddsMultipleEntitiesToDatabase_WithPassedInGuid()
			{
				var guid1 = Guid.NewGuid();
				Animal a1 = new Animal { Id = guid1, Name = "Foo" };
				var guid2 = Guid.NewGuid();
				Animal a2 = new Animal { Id = guid2, Name = "Bar" };
				var guid3 = Guid.NewGuid();
				Animal a3 = new Animal { Id = guid3, Name = "Baz" };

				Db.Insert<Animal>(new[] { a1, a2, a3 });

				var animals = Db.GetList<Animal>().ToList();
				Assert.AreEqual(3, animals.Count);
				Assert.IsNotNull(animals.FirstOrDefault(x => x.Id == guid1));
				Assert.IsNotNull(animals.FirstOrDefault(x => x.Id == guid2));
				Assert.IsNotNull(animals.FirstOrDefault(x => x.Id == guid3));
			}
		}

		[TestFixture]
		public class GetMethod : SqliteBaseFixture
		{
			[Test]
			public void UsingKey_ReturnsEntity()
			{
				Person p1 = new Person
				{
					Active = true,
					FirstName = "Foo",
					LastName = "Bar",
					DateCreated = DateTime.UtcNow
				};
				Db.Insert(p1);

				Person p2 = Db.Get<Person>(p1.Id);
				Assert.AreEqual(p1.Id, p2.Id);
				Assert.AreEqual("Foo", p2.FirstName);
				Assert.AreEqual("Bar", p2.LastName);
			}

			[Test]
			//TODO: multikey identity
			public void UsingCompositeKey_ReturnsEntity()
			{
				Multikey m1 = new Multikey { Key1 = 1, Key2 = "key", Value = "bar" };
				Db.Insert(m1);

				Multikey m2 = Db.Get<Multikey>(new { m1.Key1, m1.Key2 });
				Assert.AreEqual(1, m2.Key1);
				Assert.AreEqual("key", m2.Key2);
				Assert.AreEqual("bar", m2.Value);
			}
		}

		[TestFixture]
		public class DeleteMethod : SqliteBaseFixture
		{
			[Test]
			public void UsingKey_DeletesFromDatabase()
			{
				Person p1 = new Person
				{
					Active = true,
					FirstName = "Foo",
					LastName = "Bar",
					DateCreated = DateTime.UtcNow
				};
				Db.Insert(p1);
				var id = p1.Id;

				Person p2 = Db.Get<Person>(id);
				Db.Delete(p2);
				Assert.IsNull(Db.Get<Person>(id));
			}

			//[Test]
			//[Ignore] //TODO: multikey identity
			//public void UsingCompositeKey_DeletesFromDatabase()
			//{
			//    Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
			//    var key = Db.Insert(m1);

			//    Multikey m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 });
			//    Db.Delete(m2);
			//    Assert.IsNull(Db.Get<Multikey>(new { key.Key1, key.Key2 }));
			//}

			[Test]
			public void UsingPredicate_DeletesRows()
			{
				Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
				Person p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
				Person p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
				Db.Insert(p1);
				Db.Insert(p2);
				Db.Insert(p3);

				var list = Db.GetList<Person>();
				Assert.AreEqual(3, list.Count());

				IPredicate pred = Predicates.Predicates.Field<Person>(p => p.LastName, Operator.Eq, "Bar");
				var result = Db.Delete<Person>(pred);
				Assert.IsTrue(result);

				list = Db.GetList<Person>();
				Assert.AreEqual(1, list.Count());
			}

			[Test]
			public void UsingObject_DeletesRows()
			{
				Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
				Person p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
				Person p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
				Db.Insert(p1);
				Db.Insert(p2);
				Db.Insert(p3);

				var list = Db.GetList<Person>();
				Assert.AreEqual(3, list.Count());

				var result = Db.Delete<Person>(new { LastName = "Bar" });
				Assert.IsTrue(result);

				list = Db.GetList<Person>();
				Assert.AreEqual(1, list.Count());
			}
		}

		[TestFixture]
		public class UpdateMethod : SqliteBaseFixture
		{
			[Test]
			public void UsingKey_UpdatesEntity()
			{
				Person p1 = new Person
				{
					Active = true,
					FirstName = "Foo",
					LastName = "Bar",
					DateCreated = DateTime.UtcNow
				};
				Db.Insert(p1);

				var id = p1.Id;

				var p2 = Db.Get<Person>(id);
				p2.FirstName = "Baz";
				p2.Active = false;

				Db.Update(p2);

				var p3 = Db.Get<Person>(id);
				Assert.AreEqual("Baz", p3.FirstName);
				Assert.AreEqual("Bar", p3.LastName);
				Assert.AreEqual(false, p3.Active);
			}

			[Test]
			public void Update_WhenPartialUpdate_ShouldOnlyUpdateThePropertyUsed()
			{
				Person p1 = new Person
				{
					Active = true,
					FirstName = "Foo",
					LastName = "Bar",
					DateCreated = DateTime.UtcNow
				};

				Db.Insert(p1);

				var id = p1.Id;

				var idPredicate = Predicates.Predicates.Field<Person>(x => x.Id, Operator.Eq, id);
				var projectionSet = Projections.Set<Person>(x => x.FirstName, "FooBar");
				var list = new List<IProjectionSet>() { projectionSet };

				Db.Update<Person>(idPredicate, list, null, false);

				var p2 = Db.Get<Person>(id);

				Assert.AreEqual("FooBar", p2.FirstName);
				Assert.AreEqual(p1.LastName, p2.LastName);
				Assert.AreEqual(p1.Active, p2.Active);
			}

			//[Test]
			//[Ignore] //TODO: multikey identity
			//public void UsingCompositeKey_UpdatesEntity()
			//{
			//    Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
			//    var key = Db.Insert(m1);

			//    Multikey m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 });
			//    m2.Key2 = "key";
			//    m2.Value = "barz";
			//    Db.Update(m2);

			//    Multikey m3 = Db.Get<Multikey>(new { Key1 = 1, Key2 = "key" });
			//    Assert.AreEqual(1, m3.Key1);
			//    Assert.AreEqual("key", m3.Key2);
			//    Assert.AreEqual("barz", m3.Value);
			//}
		}

		[TestFixture]
		public class GetListMethod : SqliteBaseFixture
		{
			[Test]
			public void UsingNullPredicate_ReturnsAll()
			{
				Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
				Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
				Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
				Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

				IEnumerable<Person> list = Db.GetList<Person>();
				Assert.AreEqual(4, list.Count());
			}

			[Test]
			public void UsingPredicate_ReturnsMatching()
			{
				Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
				Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
				Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
				Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

				var predicate = Predicates.Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
				IEnumerable<Person> list = Db.GetList<Person>(predicate, null);
				Assert.AreEqual(2, list.Count());
				Assert.IsTrue(list.All(p => p.FirstName == "a" || p.FirstName == "c"));
			}

			[Test]
			public void UsingObject_ReturnsMatching()
			{
				Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
				Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
				Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
				Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

				var predicate = new { Active = true, FirstName = "c" };
				IEnumerable<Person> list = Db.GetList<Person>(predicate, null);
				Assert.AreEqual(1, list.Count());
				Assert.IsTrue(list.All(p => p.FirstName == "c"));
			}

			[Test]
			public void UsingProjections_Returns_ChosenField()
			{
				Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });

				var projections = new List<IProjection> { Projections.Projection<Person>(x => x.FirstName) };

				var results = Db.GetList<Person>(projections: projections);

				Assert.AreEqual(results.Count(), 1);

				var result = results.Single();

				Assert.AreEqual(result.FirstName, "a");
				Assert.IsNull(result.LastName);
				Assert.IsFalse(result.Active);
				Assert.AreEqual(result.DateCreated, default(DateTime));
			}
		}
	}

	[TestFixture]
	public class GetPageMethod : SqliteBaseFixture
	{
		[Test]
		public void UsingNullPredicate_ReturnsMatching()
		{
			var p1 = new Person
			{
				Active = true,
				FirstName = "Sigma",
				LastName = "Alpha",
				DateCreated = DateTime.UtcNow

			};
			var p2 = new Person
			{
				Active = false,
				FirstName = "Delta",
				LastName = "Alpha",
				DateCreated = DateTime.UtcNow

			};
			var p3 = new Person
			{
				Active = true,
				FirstName = "Theta",
				LastName = "Gamma",
				DateCreated = DateTime.UtcNow
			};
			var p4 = new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow };


			Db.Insert(p1);
			Db.Insert(p2);
			Db.Insert(p3);
			Db.Insert(p4);

			IList<ISort> sort = new List<ISort>
									{
										Predicates.Predicates.Sort<Person>(p => p.LastName),
										Predicates.Predicates.Sort<Person>(p => p.FirstName)
									};

			IEnumerable<Person> list = Db.GetPage<Person>(null, sort, 0, 2);
			Assert.AreEqual(2, list.Count());
			Assert.AreEqual(p2.Id, list.First().Id);
			Assert.AreEqual(p1.Id, list.Skip(1).First().Id);
		}

		[Test]
		public void UsingPredicate_ReturnsMatching()
		{
			var p1 = new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow };
			var p2 = new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow };
			var p3 = new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow };
			var p4 = new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow };

			Db.Insert(p1);
			Db.Insert(p2);
			Db.Insert(p3);
			Db.Insert(p4);

			var predicate = Predicates.Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
			IList<ISort> sort = new List<ISort>
									{
										Predicates.Predicates.Sort<Person>(p => p.LastName),
										Predicates.Predicates.Sort<Person>(p => p.FirstName)
									};

			IEnumerable<Person> list = Db.GetPage<Person>(predicate, sort, 0, 3);
			Assert.AreEqual(2, list.Count());
			Assert.IsTrue(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
		}

		[Test]
		public void NotFirstPage_Returns_NextResults()
		{
			var p1 = new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow };
			var p2 = new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow };
			var p3 = new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow };
			var p4 = new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow };

			Db.Insert(p1);
			Db.Insert(p2);
			Db.Insert(p3);
			Db.Insert(p4);

			IList<ISort> sort = new List<ISort>
									{
										Predicates.Predicates.Sort<Person>(p => p.LastName),
										Predicates.Predicates.Sort<Person>(p => p.FirstName)
									};

			IEnumerable<Person> list = Db.GetPage<Person>(null, sort, 1, 2);
			Assert.AreEqual(2, list.Count());
			Assert.AreEqual(p4.Id, list.First().Id);
			Assert.AreEqual(p3.Id, list.Skip(1).First().Id);
		}

		[Test]
		public void UsingObject_ReturnsMatching()
		{
			var p1 = new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow };
			var p2 = new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow };
			var p3 = new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow };
			var p4 = new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow };

			Db.Insert(p1);
			Db.Insert(p2);
			Db.Insert(p3);
			Db.Insert(p4);

			var predicate = new { Active = true };
			IList<ISort> sort = new List<ISort>
									{
										Predicates.Predicates.Sort<Person>(p => p.LastName),
										Predicates.Predicates.Sort<Person>(p => p.FirstName)
									};

			IEnumerable<Person> list = Db.GetPage<Person>(predicate, sort, 0, 3);
			Assert.AreEqual(2, list.Count());
			Assert.IsTrue(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
		}
	}

	[TestFixture]
	public class CountMethod : SqliteBaseFixture
	{
		[Test]
		public void UsingNullPredicate_Returns_Count()
		{
			Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
			Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
			Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
			Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

			int count = Db.Count<Person>(null);
			Assert.AreEqual(4, count);
		}

		[Test]
		public void UsingPredicate_Returns_Count()
		{
			Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
			Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
			Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
			Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

			var predicate = Predicates.Predicates.Field<Person>(f => f.DateCreated, Operator.Lt, DateTime.UtcNow.AddDays(-5));
			int count = Db.Count<Person>(predicate);
			Assert.AreEqual(2, count);
		}

		[Test]
		public void UsingObject_Returns_Count()
		{
			Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
			Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
			Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
			Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

			var predicate = new { FirstName = new[] { "b", "d" } };
			int count = Db.Count<Person>(predicate);
			Assert.AreEqual(2, count);
		}
	}

	[TestFixture]
	public class GetMultipleMethod : SqliteBaseFixture
	{
		[Test]
		public void ReturnsItems()
		{
			Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
			Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
			Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
			Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

			Db.Insert(new Animal { Name = "Foo" });
			Db.Insert(new Animal { Name = "Bar" });
			Db.Insert(new Animal { Name = "Baz" });

			GetMultiplePredicate predicate = new GetMultiplePredicate();
			predicate.Add<Person>(null);
			predicate.Add<Animal>(Predicates.Predicates.Field<Animal>(a => a.Name, Operator.Like, "Ba%"));
			predicate.Add<Person>(Predicates.Predicates.Field<Person>(a => a.LastName, Operator.Eq, "c1"));

			var result = Db.GetMultiple(predicate);
			var people = result.Read<Person>().ToList();
			var animals = result.Read<Animal>().ToList();
			var people2 = result.Read<Person>().ToList();

			Assert.AreEqual(4, people.Count);
			Assert.AreEqual(2, animals.Count);
			Assert.AreEqual(1, people2.Count);
		}
	}


	[TestFixture]
	public class AtomicIncrementMethod : SqliteBaseFixture
	{
		[Test]
		public void ReturnsItems()
		{
			const int amount = 2;

			var car = new Car { Hand = 0, Name = "Car", Id = "Id" };

			Db.Insert(car);

			var fieldPredicate = Predicates.Predicates.Field<Car>(x => x.Id, Operator.Eq, car.Id);
			var projection = Projections.Projection<Car>(x => x.Hand);
			Db.AtomicIncrement<Car>(fieldPredicate, projection, amount, null);

			var result = Db.Get<Car>(car.Id);

			Assert.AreEqual(result.Hand, car.Hand + amount);
		}
	}
}