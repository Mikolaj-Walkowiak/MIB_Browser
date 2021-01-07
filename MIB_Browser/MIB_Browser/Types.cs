using System;
using System.Collections.Generic;

public enum ConstraintRangeType
{
	NONE,
	EXPLICIT,
	SIZE,
	RANGE
}
public class Constraint
{

	public Constraint(ConstraintRangeType Type, string ParentType, int Min, int Max, String Location)
	{
		rangeType = Type;
		parentType = ParentType;
		min = Min;
		max = Max;
		location = Location;
	}
	public Constraint(ConstraintRangeType Type, string ParentType, int Singular, String Location)
	{
		rangeType = Type;
		parentType = ParentType;
		min = Singular;
		max = Singular;
		location = Location;
	}


	public ConstraintRangeType rangeType { get; }
	public string parentType { get; }
	public int min { get; }
	public int max { get; }
	public string location { get; } // APLICATION 1
}