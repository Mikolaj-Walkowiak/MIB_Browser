using System;
using System.Collections.Generic;

public class Constraint
{

	public Constraint(Boolean IsSize, Boolean IsExplicit, string ParentType, string Min, string Max, String Location)
	{
		isSize = IsSize;
		isExplicit = IsExplicit;
		parentType = ParentType;
		min = Min;
		max = Max;
		location = Location;
	}
	public Constraint(Boolean IsSize, Boolean IsExplicit, string ParentType, string Singular, String Location)
	{
		isSize = IsSize;
		isExplicit = IsExplicit;
		parentType = ParentType;
		min = Singular;
		max = Singular;
		location = Location;
	}


	public Boolean isSize { get; }
	public Boolean isExplicit { get; }
	public string parentType { get; }
	public string min { get; }
	public string max { get; }
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
		return new Constraint(false,false,"negro", "-2137", "2137",null); //shouldn't be possible
    }
}
