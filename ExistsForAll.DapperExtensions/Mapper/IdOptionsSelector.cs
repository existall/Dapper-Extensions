﻿namespace ExistsForAll.DapperExtensions.Mapper
{
	public class IdOptionsSelector
	{
		private readonly PropertyMap _property;

		internal IdOptionsSelector(PropertyMap property)
		{
			_property = property;
			_property.ReadOnly()
				.Ignore();
		}

		public void AutoGenerated()
		{
		}

		public void Assigned()
		{
			_property.NotReadOnly()
				.NotIgnore();
		}
	}
}