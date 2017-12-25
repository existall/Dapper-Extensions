namespace ExistsForAll.DapperExtensions
{
	public interface IProjectionSet
	{
		string PropertyName { get; }
		object Value { get; }
	}

	internal class ProjectionSet : IProjectionSet
	{
		public string PropertyName { get; }
		public object Value { get; }

		public ProjectionSet(string propertyName, object value)
		{
			PropertyName = propertyName;
			Value = value;
		}
	}
}