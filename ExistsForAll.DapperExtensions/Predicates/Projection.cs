namespace ExistsForAll.DapperExtensions.Predicates
{
	public class Projection : IProjection
	{
		public Projection(string propertyName)
		{
			PropertyName = propertyName;
		}

		public string PropertyName { get; }
	}
}