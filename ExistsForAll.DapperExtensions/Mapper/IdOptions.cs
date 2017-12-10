namespace ExistsForAll.DapperExtensions.Mapper
{
	public class IdOptions
	{
		private readonly PropertyMap _property;

		internal IdOptions(PropertyMap property)
		{
			_property = property;
			GeneratedBy = new IdOptionsSelector(property);
		}

		public IdOptionsSelector GeneratedBy { get; }

		public IdOptions Column(string columnName)
		{
			_property.Column(columnName);
			return this;
		}
	}
}