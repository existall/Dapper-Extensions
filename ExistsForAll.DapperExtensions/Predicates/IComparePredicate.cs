﻿namespace ExistsForAll.DapperExtensions
{
	public interface IComparePredicate : IBasePredicate
	{
		Operator Operator { get; set; }
		bool Not { get; set; }
	}
}