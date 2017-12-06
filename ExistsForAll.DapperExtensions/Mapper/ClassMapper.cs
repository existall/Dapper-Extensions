using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExistsForAll.DapperExtensions.Mapper
{
	/// <summary>
	/// Maps an entity to a table through a collection of property maps.
	/// </summary>
	public class ClassMapper<T> : ClassMapperBase, IClassMapper<T> where T : class
	{
		/// <summary>
		/// Gets or sets the schema to use when referring to the corresponding table name in the database.
		/// </summary>
		public string SchemaName { get; protected set; }

		/// <summary>
		/// Gets or sets the table to use in the database.
		/// </summary>
		public string TableName { get; protected set; }

		/// <summary>
		/// A collection of properties that will map to columns in the database table.
		/// </summary>
		public IPropertyMapCollection Properties { get; }

		public IPropertyMapCollection Keys { get; }

		public Type EntityType => typeof(T);

		public ClassMapper()
		{
			Properties = new PropertyMapCollection();
			Keys = new PropertyMapCollection();
			Table(typeof(T).Name);
		}

		public virtual void Schema(string schemaName)
		{
			SchemaName = schemaName;
		}

		public virtual void Table(string tableName)
		{
			TableName = tableName;
		}

		protected IdOptions Key(Expression<Func<T, object>> expression, KeyType keyType)
		{
			var propertyInfo = ReflectionHelper<T>.GetProperty(expression) as PropertyInfo;
			var property = new PropertyMap(propertyInfo) { KeyType = keyType };
			GuardForDuplicatePropertyMap(property);
			Keys.Add(property);
			return new IdOptions(property);
		}

		/// <summary>
		/// Fluently, maps an entity property to a column
		/// </summary>
		protected PropertyMap Map(Expression<Func<T, object>> expression)
		{
			var propertyInfo = ReflectionHelper<T>.GetProperty(expression) as PropertyInfo;
			return Map(propertyInfo);
		}

		/// <summary>
		/// Fluently, maps an entity property to a column
		/// </summary>
		protected PropertyMap Map(PropertyInfo propertyInfo)
		{
			var result = new PropertyMap(propertyInfo);
			GuardForDuplicatePropertyMap(result);
			Properties.Add(result);
			return result;
		}

		/// <summary>
		/// Removes a propertymap entry
		/// </summary>
		/// <param name="expression"></param>
		protected void UnMap(Expression<Func<T, object>> expression)
		{
			var propertyInfo = ReflectionHelper<T>.GetProperty(expression) as PropertyInfo;
			var mapping = Properties.SingleOrDefault(w => w.Name == propertyInfo.Name);

			if (mapping == null)
			{
				throw new InvalidOperationException("Unable to UnMap because mapping does not exist.");
			}

			Properties.Remove(mapping);
		}

		private void GuardForDuplicatePropertyMap(PropertyMap result)
		{
			if (Properties.Any(p => p.Name.Equals(result.Name)))
			{
				throw new ArgumentException(string.Format("Duplicate mapping for property {0} detected.", result.Name));
			}
		}
	}

	public class StringMapper : ClassMapper<String>
	{
		public StringMapper()
		{
			Key(x => x.Length, KeyType.Assigned).Column("");
			Map(x => x.Length).Column("");

		}
	}
}