using System;
using System.Collections.Generic;

public class Constraint
{
	public Constraint(Boolean IsSize, string Min, string Max)
	{
		isSize = IsSize;
		min = Min;
		max = Max;
	}
	public Constraint(Boolean IsSize, string Singular)
	{
		isSize = IsSize;
		min = Singular;
		max = Singular;
	}

	public Boolean isSize { get; }
	public string min { get; }
	public string max { get; }
}

public static class Types
{
	public static Dictionary<String, Constraint> types = new Dictionary<String, Constraint>();

	public static void AddNew(String Name, Boolean IsSize, String Min, String Max)
	{
		types.Add(Name, new Constraint(IsSize, Min, Max));
	}
	public static Constraint GetConstraints(String Name)
    {
		if (types.ContainsKey(Name))
		{
			return types[Name];
		}
		return new Constraint(false, "-2137", "2137"); //TODO xd
    }
}
