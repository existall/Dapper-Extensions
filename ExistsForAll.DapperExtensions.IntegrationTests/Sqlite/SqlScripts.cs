namespace DapperExtensions.Test.IntegrationTests.Sqlite
{
	public static class SqlScripts
	{
		public static string Car = @"CREATE TABLE Car (
    Id NVARCHAR(15) PRIMARY KEY,
    Name NVARCHAR(50)
)";

		public static string Foo = @"CREATE TABLE FooTable (
    FooId INTEGER PRIMARY KEY AUTOINCREMENT,  -- In SQLITE3, this is the alias for ROWID
    [First] NVARCHAR(50),
    [Last] NVARCHAR(50),
    BirthDate DATETIME
    /*PRIMARY KEY(Key1, Key2)*/                 -- SQLite3 does not support an autoincrement in a composite key
)";

		public static string Multikey = @"CREATE TABLE Multikey (
    Key1 INTEGER NOT NULL,
    Key2 NVARCHAR(50) NOT NULL,
    Value NVARCHAR(50),
    PRIMARY KEY(Key1, Key2)
)";

		public static string Person = @"CREATE TABLE Person (
    Id INTEGER PRIMARY KEY, -- In SQLITE3, this is the alias for ROWID
    FirstName NVARCHAR(50),
    LastName NVARCHAR(50),
    DateCreated DATETIME,
    Active BIT
)";

		public static string Animal = @"CREATE TABLE Animal (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(50)
)";
	}
}