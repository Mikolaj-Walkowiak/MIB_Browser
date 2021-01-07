using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Expressions
{
	private const string br = " ";
	public static Regex comment = new Regex(@"--.*\n");
	public static Regex word = new Regex(@"[^ ]* ");

	public static Regex header = new Regex(@"^[^ ]+ DEFINITIONS ::= BEGIN ");

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
		);
	}

	public class Imports
    {
		public static Regex tag = new Regex("^" +
									"IMPORTS (.+?);"
		);

		public static Regex singleImport = new Regex(
										"(.+?) FROM (.+?);? "
		);
	}

	public class ObjectId
    {
		public static Regex tag = new Regex("^" +
									"(?<OID>\\S+) OBJECT IDENTIFIER ::= {(?<address>[^}]*)}"
		);
	}

	public class Path
    {
		public static Regex mixed = new Regex("([^(]+)\\(([0-9]+)\\)");
	}

	public class Types
    {
		public static Regex standardType = new Regex("^(?<name>\\w+) ::= \\[(?<INBO>[^\\]]+)\\] (?<type>[\\w\\s]+)\\((?<range>.*?)\\) ");
		public static Regex sequenceType = new Regex("^(?<name>\\w+) ::= SEQUENCE {(?<content>[^}]+)}");
		public static Regex choiceType = new Regex("^(?<name>\\w+) ::= CHOICE {(?<content>[^}]+)}");
		public static Regex syntaxCheck = new Regex("^(?<type>\\w+) (?<range>.*?)\\)?\\s*$");

		public class Ranges
        {
			public static Regex minmaxRange = new Regex(@"([0-9]+)\\.\\.([0-9]+)");
			public static Regex explicitRange = new Regex(@"^([0-9]+)$");
			public static Regex sizeMinmaxRange = new Regex(@"SIZE ?\\(([0-9]+)\\.\\.([0-9]+)\\)");
			public static Regex sizeExplicitRange = new Regex(@"SIZE ?\\(([0-9]+)\\)");
			public static List<Tuple<ConstraintRangeType, Regex>> ranges = new List<Tuple<ConstraintRangeType, Regex>> {
				new Tuple<ConstraintRangeType, Regex>(ConstraintRangeType.RANGE, minmaxRange),
				new Tuple<ConstraintRangeType, Regex>(ConstraintRangeType.EXPLICIT, explicitRange),
				new Tuple<ConstraintRangeType, Regex>(ConstraintRangeType.SIZE_RANGE, sizeMinmaxRange),
				new Tuple<ConstraintRangeType, Regex>(ConstraintRangeType.SIZE_EXPLICIT, sizeExplicitRange),
			};
		}
	}
}

