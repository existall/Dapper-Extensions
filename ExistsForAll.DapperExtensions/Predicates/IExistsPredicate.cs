﻿namespace ExistsForAll.DapperExtensions.Predicates
{
	public interface IExistsPredicate : IPredicate
	{
		IPredicate Predicate { get; set; }
		bool Not { get; set; }
	}
}