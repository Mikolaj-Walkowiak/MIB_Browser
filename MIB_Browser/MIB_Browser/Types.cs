using System;
using System.Collections.Generic;

public enum ConstraintRangeType
{
	NONE = 0b0000,
	EXPLICIT = 0b0001,
	RANGE = 0b0010,
	SIZE_EXPLICIT = 0b0101,
	SIZE_RANGE = 0b0110
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

	bool isSize { get; }
	public ConstraintRangeType rangeType { get; }
	public string parentType { get; }
	public int min { get; }
	public int max { get; }
	public string location { get; } // APLICATION 1
}