using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Expressions
{
	private const string br = " ";
	public static Regex comment = new Regex(@"--.*\n");

	public class ObjectType
    {
		public static Regex tag = new Regex("^" +
									"(?<name>\\S+) OBJECT-TYPE " +
										"(?:" +
											"SYNTAX (?<syntax>.+?) |" +
											"(?:ACCESS|MAX-ACCESS) (?<access>.+?) |" +
											"STATUS (?<status>.+?) |" +
											"DESCRIPTION \"(?<desc>[^\"]*)\" |" +
											"INDEX {(?<index>[^}]*)} |" +
										")*" +
									"::=" + br + "{(?<address>[^}]*)}"
		, RegexOptions.Singleline);
	}

	public class Imports
    {
		public static Regex tag = new Regex("^" +
									"IMPORTS (.+?);"
		, RegexOptions.Singleline);

		public static Regex singleImport = new Regex(
										"(.+?) FROM (.+?);? "
			, RegexOptions.Singleline);
	}

	public class ObjectId
    {
		public static Regex tag = new Regex("^" +
									"(?<OID>\\S+) OBJECT IDENTIFIER ::= {(?<address>[^}]*)}"
		, RegexOptions.Singleline);
	}

	public class Path
    {
		public static Regex mixed = new Regex("([^(]+)\\(([0-9]+)\\)");
	}
}
