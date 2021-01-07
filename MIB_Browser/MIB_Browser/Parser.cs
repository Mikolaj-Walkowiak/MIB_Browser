using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class Parser
{
    private ASPFile file;
	public Parser(ASPFile f)
	{
        file = f;
	}

    public void parse(string str)
    {
        List<Tuple<Regex, Action<Match>>> tags = new List<Tuple<Regex, Action<Match>>>(new Tuple<Regex, Action<Match>>[]{
            new Tuple<Regex, Action<Match>>(Expressions.header, new Action<Match>((Match match) => { })),
            new Tuple<Regex, Action<Match>>(Expressions.Imports.tag, new Action<Match>(parseImport)),
            new Tuple<Regex, Action<Match>>(Expressions.ObjectType.tag, new Action<Match>(parseObjectType)),
            new Tuple<Regex, Action<Match>>(Expressions.ObjectId.tag, new Action<Match>(parseObjectId)),
            new Tuple<Regex, Action<Match>>(Expressions.Types.standardType, new Action<Match>(parseStandardType)),
            new Tuple<Regex, Action<Match>>(Expressions.Types.sequenceType, new Action<Match>(parseSequenceType)),
            new Tuple<Regex, Action<Match>>(Expressions.Types.choiceType, new Action<Match>(parseChoiceType)),

            new Tuple<Regex, Action<Match>>(Expressions.word, new Action<Match>((Match match) => { })), // fallback
        });
        while (str.Length > 0)
        {
            foreach (var tag in tags)
            {
                Match match = tag.Item1.Match(str);
                if (match.Success)
                {
                    tag.Item2(match);
                    str = str.Substring(match.Length);
                    break;
                }
            }
        }
    }

    private void parseChoiceType(Match match)
    {
        //throw new NotImplementedException();
    }

    private void parseSequenceType(Match match)
    {
        //throw new NotImplementedException();
    }

    private void parseStandardType(Match match)
    {
        

        file.AddType(match.Groups["name"].Value, rangeType, match.Groups["type"].Value, min, max, match.Groups["INBO"].Value);
    }

    private void parseImport(Match match)
    {
        string matchStr = match.Groups[1].Value + "\n";
        for (Match importMatch = Expressions.Imports.singleImport.Match(matchStr); importMatch.Success; importMatch = importMatch.NextMatch())
        {
            string fileName = importMatch.Groups[2].Value + ".txt";
            if (File.Exists(fileName))
            {
                ASPFile importFile = new ASPFile(fileName);
                foreach (string import in importMatch.Groups[1].Value.Trim().Split(","))
                {
                    ITreeNode node = importFile.findNode(import.Trim());
                    if (node != null) file.mergeTree(node);
                }
            }
            else Console.WriteLine("file " + fileName + " not found");
        }
    }

    private void parseObjectType(Match match)
    {
        ObjectType objType = new ObjectType();
        objType.name = match.Groups["name"].Value;
        objType.description = match.Groups["desc"].Value;
        objType.type = parseObjectTypeSyntax(match.Groups["syntax"].Value);
        objType.access = match.Groups["access"].Value;
        objType.status = match.Groups["status"].Value;

        var path = match.Groups["address"].Value.Trim().Split(" ");
        objType.id = Int32.Parse(path[path.Length - 1]);
        objType.parent = file.findPath(path.AsSpan(0, path.Length - 1).ToArray());
        objType.parent.addChild(objType.id, objType);
    }

    private IType parseObjectTypeSyntax(string syntax)
    {
        Match syntaxMatch = Expressions.ObjectType.Syntaxes.enumInt.Match(syntax);
        if(syntaxMatch.Success)
        {
            Dictionary<string, long> enumDict = new Dictionary<string, long>();
            foreach (string entry in syntaxMatch.Groups[1].Value.Split(","))
            {
                enumDict.Add(entry.Substring(entry.IndexOf("(")), Int64.Parse(entry.Substring(entry.IndexOf("(")+1, entry.IndexOf(")"))));
            }
            return new EnumIntegerType(enumDict);
        }

        syntaxMatch = Expressions.ObjectType.Syntaxes.withConstraint.Match(syntax);
        if (syntaxMatch.Success)
        {
            throw new NotImplementedException();
        }

        syntaxMatch = Expressions.ObjectType.Syntaxes.sequenceOf.Match(syntax);
        if (syntaxMatch.Success)
        {
            return new SequenceOfType(file.fetchType(syntaxMatch.Groups[1].Value));
        }

        return new StringType();
    }

    private void parseObjectId(Match match)
    {
        ObjectId objId = new ObjectId();
        objId.name = match.Groups["OID"].Value;

        var path = match.Groups["address"].Value.Trim().Split(" ");
        objId.id = Int32.Parse(path[path.Length - 1]);
        objId.parent = file.findPath(path.AsSpan(0, path.Length - 1).ToArray());
        objId.parent.addChild(objId.id, objId);
    }

    private Tuple<long, long> parseConstraint(string cons)
    {

    }
}
