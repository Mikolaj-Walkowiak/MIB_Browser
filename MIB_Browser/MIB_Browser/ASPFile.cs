using System;
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
        while(file.Length > 0)
        {
            Match match;

            match = Expressions.Imports.tag.Match(file);
            if (match.Success)
            {
                parseImport(match);
                file = file.Substring(match.Length);
                continue;
            }

            match = Expressions.ObjectType.tag.Match(file);
            if (match.Success)
            {
                parseObjectType(match);
                file = file.Substring(match.Length);
                continue;
            }

            match = Expressions.ObjectId.tag.Match(file);
            if (match.Success)
            {
                parseObjectId(match);
                file = file.Substring(match.Length);
                continue;
            }

            file = file.Substring(file.IndexOf(' ')+1);
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
        Enum.TryParse(match.Groups["access"].Value, out AccessEnum myAccess);
        objType.access = myAccess;
        Enum.TryParse(match.Groups["status"].Value, out StatusEnum myStatus);
        objType.status = myStatus;
        
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

}
