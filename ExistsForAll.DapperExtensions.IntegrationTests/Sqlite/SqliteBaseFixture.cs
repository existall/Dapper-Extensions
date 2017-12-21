using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using Dapper;
using ExistsForAll.DapperExtensions.Sql;
using NUnit.Framework;

namespace ExistsForAll.DapperExtensions.IntegrationTests.Sqlite
{
    public class SqliteBaseFixture
    {
        protected IDatabase Db;

        [SetUp]
        public virtual void Setup()
        {
            string connectionString = string.Format("Data Source=.\\dapperTest_{0}.sqlite", Guid.NewGuid());
            string[] connectionParts = connectionString.Split(';');
            string file = connectionParts
                .ToDictionary(k => k.Split('=')[0], v => v.Split('=')[1])
                .Where(d => d.Key.Equals("Data Source", StringComparison.OrdinalIgnoreCase))
                .Select(k => k.Value).Single();

            if (File.Exists(file))
            {
                File.Delete(file);
            }

            SQLiteConnection connection = new SQLiteConnection(connectionString);

	        var config = new DapperExtensionsConfiguration
	        {
		        Dialect = new SqliteDialect(), 
		        AutoPopulateKeyGuidValue = true
	        };

	        var dapper = new DapperExtensionsBuilder().BuildImplementor(new[] {GetType().Assembly}, config);
            Db = new Database(connection, dapper.DapperImplementor);

            var files = new List<string>
                                {
                                    SqlScripts.Animal,
	                                SqlScripts.Foo,
	                                SqlScripts.Multikey,
	                                SqlScripts.Person,
									SqlScripts.Car
								};

            foreach (var setupFile in files)
            {
                connection.Execute(setupFile);
            }
        }

        [TearDown]
        public void TearDown()
        {
            string databaseName = Db.Connection.Database;
            if (!File.Exists(databaseName))
            {
                return;
            }

            int i = 10;
            while (IsDatabaseInUse(databaseName) && i > 0)
            {
                i--;
                Thread.Sleep(1000);
            }

            if (i > 0)
            {
                File.Delete(databaseName);
            }
        }

        public static bool IsDatabaseInUse(string databaseName)
        {
            FileStream fs = null;
            try
            {
                FileInfo fi = new FileInfo(databaseName);
                fs = fi.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch (Exception)
            {
                return true;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        public string ReadScriptFile(string name)
        {
            string fileName = GetType().Namespace + ".Sql." + name + ".sql";
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }
    }
}