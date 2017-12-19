﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using ExistsForAll.DapperExtensions.Mapper;
using ExistsForAll.DapperExtensions.Sql;
using ExistsForAll.DapperExtensions.UnitTests.Mapping;
using NSubstitute;
using Xunit;

namespace ExistsForAll.DapperExtensions.UnitTests.Sql
{
	public class SqlGeneratorTests
	{
		private const string BaseSql = "SELECT Id, String, DateTime, Guid FROM table";

		[Fact]
		public void Select_WhenHaveNoPredicate_ShouldReturnSelectStatement()
		{
			var sut = BuildSut();

			var mapper = GetClassMapper();

			var dictionary = new Dictionary<string, object>();

			var result = sut.Select(mapper, null, null, dictionary);

			Assert.Equal(result, BaseSql);
		}

		[Fact]
		public void Select_WhenHavePredicate_ShouldReturnSelectStatementWithWhere()
		{
			var sut = BuildSut();

			var mapper = GetClassMapper();

			var @params = new Dictionary<string, object>();

			var predicate = Predicates.Field<IntEntity>(x => x.String, Operator.Eq, "hello");

			var result = sut.Select(mapper, predicate, null, @params);

			var expected = $"{BaseSql} WHERE (String = @String_0)";

			Assert.Equal(result, expected);
		}

		[Fact]
		public void Select_WhenHavePredicateAndSort_ShouldReturnSelectStatementWithWhereAndSort()
		{
			var sut = BuildSut();

			var mapper = GetClassMapper();

			var @params = new Dictionary<string, object>();

			var predicate = Predicates.Field<IntEntity>(x => x.String, Operator.Eq, "hello");

			var idSort = Predicates.Sort<IntEntity>(x => x.Id);
			var stringSort = Predicates.Sort<IntEntity>(x => x.String, false);

			var result = sut.Select(mapper, predicate, new[] { idSort, stringSort }, @params);

			Assert.Equal(result, $"{BaseSql} WHERE (String = @String_0) ORDER BY Id ASC, String DESC");
		}

		[Fact]
		public void SelectSet_WhenHavePredicateAndSort_ShouldReturnSelectStatementWithWhereAndSort()
		{
			const int firstResult = 10;
			const int maxResults = 100;

			var sut = BuildSut();

			var mapper = GetClassMapper();

			var @params = new Dictionary<string, object>();

			var predicate = Predicates.Field<IntEntity>(x => x.String, Operator.Eq, "hello");

			var idSort = Predicates.Sort<IntEntity>(x => x.Id);
			var stringSort = Predicates.Sort<IntEntity>(x => x.String, false);

			var result = sut.SelectSet(mapper, predicate, new[] { idSort, stringSort }, firstResult, maxResults, @params);

			Assert.Equal(result, $"{BaseSql} WHERE (String = @String_0) ORDER BY Id ASC, String DESC {TestSqlDialect.SetSql(firstResult, maxResults)}");
		}

		[Fact]
		public void SelectPage_WhenHavePredicateAndSort_ShouldReturnSelectStatementWithWhereAndSort()
		{
			const int page = 3;
			const int resultsPerPage = 100;

			var sut = BuildSut();

			var mapper = GetClassMapper();

			var @params = new Dictionary<string, object>();

			var predicate = Predicates.Field<IntEntity>(x => x.String, Operator.Eq, "hello");

			var idSort = Predicates.Sort<IntEntity>(x => x.Id);
			var stringSort = Predicates.Sort<IntEntity>(x => x.String, false);

			var result = sut.SelectPaged(mapper, predicate, new[] { idSort, stringSort }, page, resultsPerPage, @params);

			Assert.Equal(result, $"{BaseSql} WHERE (String = @String_0) ORDER BY Id ASC, String DESC {TestSqlDialect.PageSql(page, resultsPerPage)}");
		}

		[Fact]
		public void Count_WhenHavePredicate_ShouldReturnCountStatement()
		{
			var sut = BuildSut();

			var mapper = GetClassMapper();

			var @params = new Dictionary<string, object>();

			var predicate = Predicates.Field<IntEntity>(x => x.String, Operator.Eq, "hello");

			var result = sut.Count(mapper, predicate, @params);

			Assert.Equal(result, $"SELECT COUNT(*) AS `Total` FROM table WHERE (String = @String_0)");
		}

		[Fact]
		public void Insert_WhenPassingClassMapNoAutoGeneratedId_ShouldReturnInsertStatement()
		{
			var sut = BuildSut();

			var mapper = GetClassMapper();

			var result = sut.Insert(mapper);

			Assert.Equal(result, $"INSERT INTO table (Id, String, DateTime, Guid) VALUES (@Id, @String, @DateTime, @Guid)");
		}

		[Fact]
		public void Insert_WhenPassingClassMapAutoGeneratedId_ShouldReturnInsertStatement()
		{
			var sut = BuildSut();

			var mapper = GetAutoGeneratedClassMapper();

			var result = sut.Insert(mapper);

			Assert.Equal(result, $"INSERT INTO table (String, DateTime, Guid) VALUES (@String, @DateTime, @Guid) RETURNING Id INTO @IdOutParam");
		}

		[Fact]
		public void Update_WhenClassMapHasAssignedId_ShouldGetUpdateStatement()
		{
			var sut = BuildSut();

			var mapper = GetClassMapper();

			var predicate = Predicates.Field<IntEntity>(x => x.Id, Operator.Eq, 100);

			var @params = GetParams();

			var result = sut.Update(mapper, predicate, @params);

			Assert.Equal(result, $"UPDATE table SET Id = @Id, String = @String, DateTime = @DateTime, Guid = @Guid WHERE (Id = @Id_0)");
		}

		[Fact]
		public void Update_WhenClassMapHasAutoGeneratedId_ShouldGetUpdateStatement()
		{
			var sut = BuildSut();

			var mapper = GetAutoGeneratedClassMapper();

			var predicate = Predicates.Field<IntEntity>(x => x.Id, Operator.Eq, 100);

			var @params = GetParams();

			var result = sut.Update(mapper, predicate, @params);

			Assert.Equal(result, $"UPDATE table SET String = @String, DateTime = @DateTime, Guid = @Guid WHERE (Id = @Id_0)");
		}

		[Fact]
		public void Delete_ShouldGetDeleteStatement()
		{
			var sut = BuildSut();

			var mapper = GetClassMapper();

			var predicate = Predicates.Field<IntEntity>(x => x.Id, Operator.Eq, 100);

			var @params = GetParams();

			var result = sut.Delete(mapper, predicate, @params);

			Assert.Equal(result, $"DELETE FROM table WHERE (Id = @Id_0)");
		}

		[Fact]
		public void Update1_WhenClassMapHasAssignedId_ShouldGetUpdateStatement()
		{
			var sut = BuildSut();

			var mapper = GetClassMapper();

			var predicate = Predicates.Field<IntEntity>(x => x.Id, Operator.Eq, 100);

			var @params = GetParams();

			var timeSpan = Run(() =>
			{
				var result = sut.Select(mapper, null, null, @params);
			});

			var timeSpan1 = Run(() =>
			{
				var result = sut.Select(mapper, null, null, @params);
			});

			var timeSpan12 = Run(() =>
			{
				for (int i = 0; i < 10000; i++)
				{
					var result = sut.Select(mapper, null, null, @params);
				}
			});
		}

		private TimeSpan Run(Action action)
		{

			var sw = Stopwatch.StartNew();

			action();

			sw.Stop();

			return sw.Elapsed;
		}

		private SqlGenerator BuildSut()
		{
			var dapperExtensionsConfiguration = Substitute.For<IDapperExtensionsConfiguration>();
			dapperExtensionsConfiguration.Dialect.Returns(new TestSqlDialect());
			return new SqlGenerator(dapperExtensionsConfiguration);
		}

		private IClassMapper GetClassMapper()
		{
			var mapper = new IntEntityMapper();
			mapper.Table("table");
			return mapper;
		}

		private IClassMapper GetAutoGeneratedClassMapper()
		{
			var mapper = new AutoGeneratedLongEntityMapper();
			mapper.Table("table");
			return mapper;
		}

		private Dictionary<string, object> GetParams()
		{
			return new Dictionary<string, object>();
		}
	}
}
