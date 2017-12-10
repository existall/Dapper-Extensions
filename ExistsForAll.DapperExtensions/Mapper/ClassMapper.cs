﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExistsForAll.DapperExtensions.Mapper
{
	/// <summary>
	/// Maps an entity to a table through a collection of property maps.
	/// </summary>
	public class ClassMapper<T> : IClassMapper<T> where T : class
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


		public Expression<Func<object, object>> ConvertToObject<TParm, TReturn>(Expression<Func<TParm, TReturn>> input)
		{
			var parm = Expression.Parameter(typeof(object));
			var castParm = Expression.Convert(parm, typeof(TParm));
			var body = ReplaceExpression(input.Body, input.Parameters[0], castParm);
			body = Expression.Convert(body, typeof(object));
			return Expression.Lambda<Func<object, object>>(body, parm);
		}

		Expression ReplaceExpression(Expression body, Expression source, Expression dest)
		{
			var replacer = new ExpressionReplacer(source, dest);
			return replacer.Visit(body);
		}

		public class ExpressionReplacer : ExpressionVisitor
		{
			Expression _source;
			Expression _dest;

			public ExpressionReplacer(Expression source, Expression dest)
			{
				_source = source;
				_dest = dest;
			}

			public override Expression Visit(Expression node)
			{
				if (node == _source)
					return _dest;

				return base.Visit(node);
			}
		}


		protected IdOptions Key<TOut>(Expression<Func<T, TOut>> expression)
		{
			var propertyInfo = ReflectionHelper<T>.GetProperty(expression) as PropertyInfo;

			var property = new PropertyMap<T, TOut>(propertyInfo, expression.Compile(), GenerateSetterMethod(expression));
			GuardForDuplicatePropertyMap(property);
			Keys.Add(property);
			return new IdOptions(property);
		}

		private static Action<T, TOut> GenerateSetterMethod<TOut>(Expression<Func<T, TOut>> expression)
		{
			var memberExpression = (MemberExpression)expression.Body;

			var target = Expression.Parameter(typeof(T), "x");
			var value = Expression.Parameter(memberExpression.Type, "v");

			var memberProperty = Expression.MakeMemberAccess(target, memberExpression.Member);
			var assignment = Expression.Assign(memberProperty, value);

			var lambda = Expression.Lambda<Action<T, TOut>>(assignment, target, value);
			return lambda.Compile();
		}

		/// <summary>
		/// Fluently, maps an entity property to a column
		/// </summary>
		protected PropertyMap Map<TOut>(Expression<Func<T, TOut>> expression)
		{
			var propertyInfo = ReflectionHelper<T>.GetProperty(expression) as PropertyInfo;
			return Map<TOut>(propertyInfo, expression);
		}

		/// <summary>
		/// Fluently, maps an entity property to a column
		/// </summary>
		protected PropertyMap Map<TOut>(PropertyInfo propertyInfo, Expression<Func<T, TOut>> expression)
		{

			var result = new PropertyMap<T,TOut>(propertyInfo , expression.Compile(), GenerateSetterMethod<TOut>(expression));
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
}