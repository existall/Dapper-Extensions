using System;
using System.Data;
using System.Reflection;
using Dapper;

namespace ExistsForAll.DapperExtensions.Mapper
{
	public class PropertyMap<T, TOut> : PropertyMap
	{
		private readonly Action<T, TOut> _setter;

		public PropertyMap(PropertyInfo propertyInfo, Func<T, TOut> getter, Action<T, TOut> setter)
			: base(propertyInfo)
		{
			_setter = setter;
			Getter = o => getter((T)o);
		}

		public override void Setter(object entity, object value)
		{
			if (value is DBNull)
				return;

			_setter((T)entity, (TOut)value);
		}

		public override PropertyMap CustomMapper(ICustomsMapper customMapper)
		{
			var typeHandlerAdapter = new TypeHandlerAdapter(customMapper);

			var type = typeof(TOut);
			SqlMapper.AddTypeHandler(type, typeHandlerAdapter);
			return this;
		}
	}

	/// <summary>
	/// Maps an entity property to its corresponding column in the database.
	/// </summary>
	public abstract class PropertyMap : IPropertyMap
	{
		protected PropertyMap(PropertyInfo propertyInfo)
		{
			PropertyInfo = propertyInfo;
			ColumnName = PropertyInfo.Name;
		}

		/// <summary>
		/// Gets the name of the property by using the specified propertyInfo.
		/// </summary>
		public string Name => PropertyInfo.Name;

		/// <summary>
		/// Gets the column name for the current property.
		/// </summary>
		public string ColumnName { get; private set; }

		/// <summary>
		/// Gets the ignore status of the current property. If ignored, the current property will not be included in queries.
		/// </summary>
		public bool Ignored { get; private set; }

		/// <summary>
		/// Gets the read-only status of the current property. If read-only, the current property will not be included in INSERT and UPDATE queries.
		/// </summary>
		public bool IsReadOnly { get; private set; }

		public Func<object, object> Getter { get; protected set; }

		public abstract void Setter(object entity, object value);

		/// <summary>
		/// Gets the property info for the current property.
		/// </summary>
		public PropertyInfo PropertyInfo { get; }

		/// <summary>
		/// Fluently sets the column name for the property.
		/// </summary>
		/// <param name="columnName">The column name as it exists in the database.</param>
		public PropertyMap Column(string columnName)
		{
			ColumnName = columnName;
			return this;
		}

		/// <summary>
		/// Fluently sets the ignore status of the property.
		/// </summary>
		public PropertyMap Ignore()
		{
			Ignored = true;
			return this;
		}

		/// <summary>
		/// Fluently sets the read-only status of the property.
		/// </summary>
		public PropertyMap ReadOnly()
		{
			IsReadOnly = true;
			return this;
		}

		internal PropertyMap NotReadOnly()
		{
			IsReadOnly = false;
			return this;
		}

		internal PropertyMap NotIgnore()
		{
			Ignored = false;
			return this;
		}

		public abstract PropertyMap CustomMapper(ICustomsMapper customMapper);
	}

	public interface ICustomsMapper
	{
		object FromDb(Type destinationType, object input);
		object ToParameter(object input);
	}

	public class EnumCustomType : ICustomsMapper
	{
		public object FromDb(Type destinationType, object input)
		{
			return Enum.Parse(destinationType, (string)input, true);
		}

		public object ToParameter(object input)
		{
			return input.ToString();
		}
	}

	internal class TypeHandlerAdapter : SqlMapper.ITypeHandler<Enum>
	{
		private readonly ICustomsMapper _customsMapper;

		public TypeHandlerAdapter(ICustomsMapper customsMapper)
		{
			_customsMapper = customsMapper;
		}

		public void SetValue(IDbDataParameter parameter, object value)
		{
			parameter.Value = _customsMapper.ToParameter(value);
		}

		public object Parse(Type destinationType, object value)
		{
			return _customsMapper.FromDb(destinationType, value);
		}
	}

	internal class TypeHandlerAdapter : SqlMapper.ITypeHandler
	{
		private readonly ICustomsMapper _customsMapper;

		public TypeHandlerAdapter(ICustomsMapper customsMapper)
		{
			_customsMapper = customsMapper;
		}

		public void SetValue(IDbDataParameter parameter, object value)
		{
			parameter.Value = _customsMapper.ToParameter(value);
		}

		public object Parse(Type destinationType, object value)
		{
			return _customsMapper.FromDb(destinationType, value);
		}
	}
}