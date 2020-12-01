using System;
using System.Text.RegularExpressions;

public class Expressions
{

	public const string br = "(?:\\s|--.*\\n\\s)+";

	public static Regex objectType = new Regex("^" +
									"(?<name>\\w*)" + br + "OBJECT-TYPE" + br + 
										"(?:" +
											"SYNTAX" + br + "(?<syntax>.+)" + br +					"|" +
											"(?:ACCESS|MAX-ACCESS)" + br + "(?<access>.+)" + br +	"|" +
											"STATUS" + br + "(?<status>.+)" + br +					"|" +
											"DESCRIPTION" + br + "(?<desc>\"[^\"]*\")" + br +		"|" +
										")*" +
									"::=" + br + "(?<index>{[^}]*})"
		);

	public Expressions() {}
}
