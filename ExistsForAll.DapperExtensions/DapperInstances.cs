namespace ExistsForAll.DapperExtensions
{
	public class DapperInstances
	{
		public IDapperImplementor DapperImplementor { get; }
		public IDapperAsyncImplementor DapperAsyncImplementor { get; }

		public DapperInstances(IDapperImplementor dapperImplementor, IDapperAsyncImplementor dapperAsyncImplementor)
		{
			DapperImplementor = dapperImplementor;
			DapperAsyncImplementor = dapperAsyncImplementor;
		}
	}
}