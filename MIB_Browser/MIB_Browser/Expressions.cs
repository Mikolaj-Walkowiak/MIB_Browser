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

		public class Syntaxes
        {
			public static Regex enumInt = new Regex(@"^INTEGER ?{([^}]+)}$");
			public static Regex withConstraint = new Regex(@"^(?<type>OBJECT IDENTIFIER|OCTET STRING|\w+) \((?<cons>.+)\)$");
			public static Regex sequenceOf = new Regex(@"^SEQUENCE OF (.+)$");
        }
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
		public static Regex standardType = new Regex("^(?<name>\\w+) ::= (?:\\[(?<INBO>[^\\]]+)\\] )?(?:(?<ie>(IMPLICIT|EXPLICIT)) )?(?<type>OBJECT IDENTIFIER|OCTET STRING|[A-Za-z0-9]+) (?:\\((?<range>.*?)\\) )?");
		public static Regex sequenceType = new Regex("^(?<name>\\w+) ::= SEQUENCE {(?<content>[^}]+)}");
		public static Regex choiceType = new Regex("^(?<name>\\w+) ::= CHOICE {(?<content>[^}]+)}");
		public static Regex enumIntType = new Regex(@"^(?<name>\w+) ::= (?:\[(?<INBO>[^\]]+)\] )?(?:(?<ie>(IMPLICIT|EXPLICIT)) )?INTEGER {(?<content>[^}]+)}");
		public static Regex sequenceOfType = new Regex(@"^(?<name>\w+) ::= SEQUENCE OF (?<type>[^ ]+)(?: \\((?<cons>.+)\\))?");
		public static Regex syntax = new Regex("^(?<name>[^ ]+) (?<type>OBJECT IDENTIFIER|OCTET STRING|\\w+)(?: \\((?<cons>.+)\\))?$");

		public class Ranges
        {
			public static Regex range = new Regex(@"^([0-9]+)\.\.([0-9]+)");
			public static Regex setValue = new Regex(@"^([0-9]+)$");
			public static Regex sizeValue = new Regex(@"^SIZE ?\(([0-9]+)\.\.([0-9]+)\)");
			public static Regex sizeRange = new Regex(@"^SIZE ?\(([0-9]+)\)");
			public static List<Regex> ranges = new List<Regex> {
				range,
				setValue,
				sizeValue,
				sizeRange
			};
		}

		public class Sequences
        {
			public static Regex inputSplit = new Regex(@"([a-zA-Z0-9]+) ([^{},]+|{(?>{(?<c>)|[^{}]+|}(?<-c>))*(?(c)(?!))})");
			public static Regex seqOfSplit = new Regex(@"[^{},]+|{(?>{(?<c>)|[^{}]+|}(?<-c>))*(?(c)(?!))}");
		}
	}

	public class Macro
    {
		public static Regex tag = new Regex(@"^[^ ]+ MACRO ::= BEGIN.+?END");
    }
}

