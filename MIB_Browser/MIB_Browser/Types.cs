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


public static class Types
{
	public static Dictionary<String, Constraint> types = new Dictionary<String, Constraint>();

	public static void AddNew(String Name, Boolean IsSize, Boolean IsExplicit, string ParentType, String Min, String Max, String Location)
	{
		types.Add(Name, new Constraint(IsSize, IsExplicit, ParentType, Min, Max));
	}
	public static Constraint GetConstraints(String Name)
    {
		if (types.ContainsKey(Name))
		{
			return types[Name];
		}
		return new Constraint(false,false,"INTEGER", "-100", "100","APLICATION 4"); //shouldn't be possible
    }
}
