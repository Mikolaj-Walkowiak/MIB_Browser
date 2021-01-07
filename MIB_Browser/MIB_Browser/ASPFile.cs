﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class ASPFile
{
    public ITreeNode root = new ObjectId
    {
        id = 0,
        name = "root",
        children = new Dictionary<int, ITreeNode>
        {
            [0] = new ObjectId { id = 0, name = "ccitt" },
            [1] = new ObjectId
            {
                id = 1,
                name = "iso",
                children = new Dictionary<int, ITreeNode>
                {
                    [3] = new ObjectId
                    {
                        id = 3,
                        name = "org",
                        children = new Dictionary<int, ITreeNode>
                        {
                            [6] = new ObjectId
                            {
                                id = 6,
                                name = "dod"
                            }
                        }
                    }
                }
            },
            [2] = new ObjectId { id = 2, name = "joint" }
        }
    };

    public Dictionary<String, Constraint> types = new Dictionary<String, Constraint>();

    public ASPFile(string file)
    {
        setParents(root);
        Console.WriteLine("parsing file " + file);
        string input = File.ReadAllText(file);

        parseFile(preprocessFile(input));
        Console.WriteLine("file " + file + " done");
    }

    private string preprocessFile(string file)
    {
        file = Expressions.comment.Replace(file, "");
        file = Regex.Replace(file, @"\s+", " ");
        return file;
    }

    private void setParents(ITreeNode node)
    {
        foreach(ITreeNode child in node.getChildren().Values)
        {
            child.setParent(node);
            setParents(child);
        }
    }
	private void parseFile(string file)
    {
        List<Tuple<Regex, Action<Match>>> tags = new List<Tuple<Regex, Action<Match>>>(new Tuple<Regex, Action<Match>>[]{
            new Tuple<Regex, Action<Match>>(Expressions.Imports.tag, new Action<Match>(parseImport)),
            new Tuple<Regex, Action<Match>>(Expressions.ObjectType.tag, new Action<Match>(parseObjectType)),
            new Tuple<Regex, Action<Match>>(Expressions.ObjectId.tag, new Action<Match>(parseObjectId)),
            new Tuple<Regex, Action<Match>>(Expressions.Types.standardType, new Action<Match>(parseStandardType)),
            new Tuple<Regex, Action<Match>>(Expressions.Types.sequenceType, new Action<Match>(parseSequenceType)),
            new Tuple<Regex, Action<Match>>(Expressions.Types.choiceType, new Action<Match>(parseChoiceType)),

            new Tuple<Regex, Action<Match>>(Expressions.word, new Action<Match>((Match match) => { })), // fallback
        });
        while (file.Length > 0)
        {
            foreach (var tag in tags)
            {
                Match match = tag.Item1.Match(file);
                if (match.Success)
                {
                    tag.Item2(match);
                    file = file.Substring(match.Length);
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
        ConstraintRangeType rangeType = ConstraintRangeType.NONE;
        int min = 0, max = 0;
        string range = match.Groups["range"].Value;

        AddType(match.Groups["name"].Value, rangeType, match.Groups["type"].Value, min, max, match.Groups["INBO"].Value);
    }

    public ITreeNode findPath(string[] path, ITreeNode context)
    {
        ITreeNode node = context;
        int i = 0;
        if(path[0] == "root")
        {
            node = root;
            i = 1;
        }
        for (; i < path.Length; ++i)
        {
            if (path[i] == "^") node = node.getParent();
            else node = node.getChildByName(path[i]);
            if (node == null) return null;
        }
        return node;
    }
    public ITreeNode findPath(string[] path)
    {
        string pathRoot = path[0];
        ITreeNode node = findNode(root, pathRoot);
        for(int i = 1; i<path.Length; ++i)
        {
            if (path[i] == "^") node = node.getParent();
            else node = node.getChildByName(path[i]);
        }
        return node;
    }

    public ITreeNode findNode(string name) => findNode(root, name);
    public ITreeNode findNode(int id) => findNode(root, id);

    public ITreeNode findNode(ITreeNode root, string name)
    {
        ITreeNode node = root.getChildByName(name);
        if (node != null) return node;
        foreach(ITreeNode child in root.getChildren().Values)
        {
            node = findNode(child, name);
            if (node != null) return node;
        }
        return null;
    }

    public ITreeNode findNode(ITreeNode root, int id)
    {
        ITreeNode node = root.getChildByIndex(id);
        return node != null ? node : null;
    }

    private void mergeTree(ITreeNode foreignNode)
    {
        ITreeNode parent = findNode(foreignNode.getParent().getName());
        if (parent != null)
        {
            parent.addChild(foreignNode.getId(), foreignNode);
            foreignNode.setParent(parent);
        }
        else mergeTree(foreignNode.getParent());
    }

    private void parseImport(Match match)
    {
        string matchStr = match.Groups[1].Value + "\n";
        for (Match importMatch = Expressions.Imports.singleImport.Match(matchStr); importMatch.Success; importMatch = importMatch.NextMatch())
        {
            string fileName = importMatch.Groups[2].Value + ".txt";
            if (File.Exists(fileName))
            {
                ASPFile file = new ASPFile(fileName);
                foreach (string import in importMatch.Groups[1].Value.Trim().Split(","))
                {
                    ITreeNode node = file.findNode(import.Trim());
                    if (node != null) mergeTree(node);
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
        objType.syntax = match.Groups["syntax"].Value;
        objType.access = match.Groups["access"].Value;
        objType.status = match.Groups["status"].Value;
        
        var path = match.Groups["address"].Value.Trim().Split(" ");
        objType.id = Int32.Parse(path[path.Length - 1]);
        objType.parent = findPath(path.AsSpan(0, path.Length - 1).ToArray());
        objType.parent.addChild(objType.id, objType);
    }

    private void parseObjectId(Match match)
    {
        ObjectId objId = new ObjectId();
        objId.name = match.Groups["OID"].Value;

        var path = match.Groups["address"].Value.Trim().Split(" ");
        objId.id = Int32.Parse(path[path.Length - 1]);
        objId.parent = findPath(path.AsSpan(0, path.Length - 1).ToArray());
        objId.parent.addChild(objId.id, objId);
    }

    public void AddType(String Name, ConstraintRangeType RangeType, string ParentType, int Min, int Max, String Location)
    {
        types.Add(Name, new Constraint(RangeType, ParentType, Min, Max, Location));
    }
    public Constraint GetConstraints(String Name)
    {
        if (types.ContainsKey(Name))
        {
            return types[Name];
        }
        return new Constraint(ConstraintRangeType.NONE, "negro", -2137, 2137, null); //shouldn't be possible
    }

}
