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

        new Parser(this).parse(preprocessFile(input));
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

    public void mergeTree(ITreeNode foreignNode)
    {
        ITreeNode parent = findNode(foreignNode.getParent().getName());
        if (parent != null)
        {
            parent.addChild(foreignNode.getId(), foreignNode);
            foreignNode.setParent(parent);
        }
        else mergeTree(foreignNode.getParent());
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
        return new Constraint(ConstraintRangeType.SIZE_RANGE, "OCTET STRING", 0, 100, "APLICATION 45"); //shouldn't be possible
    }

}
