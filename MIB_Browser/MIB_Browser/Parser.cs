﻿using System;
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

    public void parse(string input)
    {

        List<Tuple<Regex, Action<Match>>> tags = new List<Tuple<Regex, Action<Match>>>(new Tuple<Regex, Action<Match>>[]{
            new Tuple<Regex, Action<Match>>(Expressions.header, new Action<Match>((Match match) => { })),
            new Tuple<Regex, Action<Match>>(Expressions.Macro.tag, new Action<Match>((Match match) => { })),
            new Tuple<Regex, Action<Match>>(Expressions.Imports.tag, new Action<Match>(parseImport)),
            new Tuple<Regex, Action<Match>>(Expressions.Types.choiceType, new Action<Match>(parseChoiceType)),
            new Tuple<Regex, Action<Match>>(Expressions.Types.sequenceOfType, new Action<Match>(parseSequenceOfType)),
            new Tuple<Regex, Action<Match>>(Expressions.Types.sequenceType, new Action<Match>(parseSequenceType)),
            new Tuple<Regex, Action<Match>>(Expressions.Types.standardType, new Action<Match>(parseStandardType)),

            new Tuple<Regex, Action<Match>>(Expressions.word, new Action<Match>((Match match) => { })), // fallback
        });
        string str = input;
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
        str = input;
        tags = new List<Tuple<Regex, Action<Match>>>(new Tuple<Regex, Action<Match>>[]{
            new Tuple<Regex, Action<Match>>(Expressions.ObjectType.tag, new Action<Match>(parseObjectType)),
            new Tuple<Regex, Action<Match>>(Expressions.ObjectId.tag, new Action<Match>(parseObjectId)),

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
        Dictionary<string, IType> members = new Dictionary<string, IType>();
        foreach (string el in match.Groups["content"].Value.Split(","))
        {
            Match syntaxMatch = Expressions.Types.syntax.Match(el.Trim());
            var cons = parseConstraint(syntaxMatch.Groups["cons"].Value);
            IType baseType = file.fetchType(syntaxMatch.Groups["type"].Value);
            if (cons != null) members.Add(syntaxMatch.Groups["name"].Value, baseType.derive(syntaxMatch.Groups["name"].Value, cons.Item1, cons.Item2));
            else members.Add(syntaxMatch.Groups["name"].Value, baseType);
        }
        file.AddType(new ChoiceType(match.Groups["name"].Value, members));
    }

    private void parseSequenceType(Match match)
    {
        Dictionary<string, IType> members = new Dictionary<string, IType>();
        foreach (string el in match.Groups["content"].Value.Split(","))
        {
            Match syntaxMatch = Expressions.Types.syntax.Match(el.Trim());
            var cons = parseConstraint(syntaxMatch.Groups["cons"].Value);
            IType baseType = file.fetchType(syntaxMatch.Groups["type"].Value);
            if (cons != null) members.Add(syntaxMatch.Groups["name"].Value, baseType.derive(syntaxMatch.Groups["name"].Value, cons.Item1, cons.Item2, baseType.classId, baseType.addr, baseType.isExplicit));
            else members.Add(syntaxMatch.Groups["name"].Value, baseType.derive(syntaxMatch.Groups["name"].Value, baseType.min, baseType.max, baseType.classId, baseType.addr, baseType.isExplicit));
        }
        string[] addr = match.Groups["INBO"].Value.Split(" ");
        file.AddType(new SequenceType(match.Groups["name"].Value,
            members, 
            addr.Length == 2 ? addr[0] : null, 
            addr.Length == 2 ? addr[1] : null, 
            match.Groups["ie"].Value.Length > 0 ? match.Groups["ie"].Value : null)
        );
    }

    private void parseStandardType(Match match)
    {
        IType baseType = file.fetchType(match.Groups["type"].Value);
        string[] addr = match.Groups["INBO"].Value.Split(" ");
        var cons = parseConstraint(match.Groups["range"].Value);
        IType newType = baseType.derive(
                    match.Groups["name"].Value,
                    cons != null ? cons.Item1 : Int64.MinValue, 
                    cons != null ? cons.Item1 : Int64.MaxValue,
                    addr.Length == 2 ? addr[0] : null,
                    addr.Length == 2 ? addr[1] : null,
                    match.Groups["ie"].Value.Length > 0 ? match.Groups["ie"].Value : null);
        file.AddType(newType);
    }

    private void parseImport(Match match)
    {
        string matchStr = match.Groups[1].Value + "\n";
        for (Match importMatch = Expressions.Imports.singleImport.Match(matchStr); importMatch.Success; importMatch = importMatch.NextMatch())
        {
            string fileName = importMatch.Groups[2].Value + ".txt";
            if (File.Exists(fileName))
            {
                ASPFile importFile = new ASPFile(File.ReadAllText(fileName));
                foreach (string import in importMatch.Groups[1].Value.Trim().Split(","))
                {
                    ITreeNode node = importFile.findNode(import.Trim());
                    if (node != null) file.mergeTree(node);
                    if (importFile.tryFetchType(import.Trim(), out IType type))
                    {
                        file.AddType(type);
                    }
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
        if (syntaxMatch.Success)
        {
            Dictionary<string, long> enumDict = new Dictionary<string, long>();
            foreach (string spl in syntaxMatch.Groups[1].Value.Split(","))
            {
                string entry = spl.Trim();
                enumDict.Add(entry.Substring(0, entry.IndexOf("(")), Int64.Parse(entry.Substring(entry.IndexOf("(") + 1, entry.IndexOf(")") - entry.IndexOf("(") - 1)));
            }
            return new EnumIntegerType(null, enumDict);
        }

        syntaxMatch = Expressions.ObjectType.Syntaxes.withConstraint.Match(syntax);
        if (syntaxMatch.Success)
        {
            IType baseType = file.fetchType(syntaxMatch.Groups["type"].Value);
            var cons = parseConstraint(syntaxMatch.Groups["cons"].Value);
            return baseType.derive(baseType.name, cons.Item1, cons.Item2);
        }

        syntaxMatch = Expressions.ObjectType.Syntaxes.sequenceOf.Match(syntax);
        if (syntaxMatch.Success)
        {
            return new SequenceOfType(null, file.fetchType(syntaxMatch.Groups[1].Value));
        }

        return file.fetchType(syntax);
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

    private Tuple<long, long> parseConstraint(string range)
    {
        long min = 0, max = 0;
        foreach (Regex t in Expressions.Types.Ranges.ranges)
        {
            Match rangeMatch = t.Match(range);
            if (rangeMatch.Success)
            {
                min = Int32.Parse(rangeMatch.Groups[1].Value);
                max = rangeMatch.Groups.Count > 2 ? Int64.Parse(rangeMatch.Groups[2].Value) : min;
                return new Tuple<long, long>(min, max);
            }
        }
        return null;
    }

    private void parseEnumInt(Match match)
    {
        string[] addr = match.Groups["INBO"].Value.Split(" ");
        Dictionary<string, long> enumDict = new Dictionary<string, long>();
        foreach (string spl in match.Groups["content"].Value.Split(","))
        {
            string entry = spl.Trim();
            enumDict.Add(entry.Substring(0, entry.IndexOf("(")), Int64.Parse(entry.Substring(entry.IndexOf("(") + 1, entry.IndexOf(")") - entry.IndexOf("(") - 1)));
        }
        file.AddType(
            new EnumIntegerType(match.Groups["name"].Value, 
                enumDict, 
                addr.Length == 2 ? addr[0] : null,
                addr.Length == 2 ? addr[1] : null,
                match.Groups["ie"].Value.Length > 0 ? match.Groups["ie"].Value : null)
        );
    }

    private void parseSequenceOfType(Match match)
    {
        var cons = parseConstraint(match.Groups["cons"].Value);
        IType baseType = file.fetchType(match.Groups["type"].Value);
        if (cons != null) file.AddType(new SequenceOfType(match.Groups["name"].Value, baseType.derive(baseType.name, cons.Item1, cons.Item2)));
        else file.AddType(new SequenceOfType(match.Groups["name"].Value, file.fetchType(match.Groups["type"].Value)));
    }

    public void parseAnyType(string line)
    {
        List<Tuple<Regex, Action<Match>>> tags = new List<Tuple<Regex, Action<Match>>>(new Tuple<Regex, Action<Match>>[]{
            new Tuple<Regex, Action<Match>>(Expressions.Types.choiceType, new Action<Match>(parseChoiceType)),
            new Tuple<Regex, Action<Match>>(Expressions.Types.sequenceOfType, new Action<Match>(parseSequenceOfType)),
            new Tuple<Regex, Action<Match>>(Expressions.Types.sequenceType, new Action<Match>(parseSequenceType)),
            new Tuple<Regex, Action<Match>>(Expressions.Types.enumIntType, new Action<Match>(parseEnumInt)),
            new Tuple<Regex, Action<Match>>(Expressions.Types.standardType, new Action<Match>(parseStandardType)),
        });
        foreach (var tag in tags)
        {
            Match match = tag.Item1.Match(line);
            if (match.Success)
            {
                tag.Item2(match);
                return;
            }
        }
    }
}
