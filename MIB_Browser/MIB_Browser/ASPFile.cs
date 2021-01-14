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

    public Dictionary<String, IType> types = new Dictionary<String, IType>
    {
        ["INTEGER"] = new IntegerType(Int64.MinValue, Int64.MaxValue, null, null, null),
        ["OCTET STRING"] = new StringType(0, Int64.MaxValue, null, null, null),
        ["OBJECT IDENTIFIER"] = new OIDType(Int64.MinValue, Int64.MaxValue, null, null, null),
        ["NULL"] = new NullType(null, null),
        ["BOOLEAN"] = new BoolType(null, null, null)
    };

    public Dictionary<long, IType> basicTypes;
    public void initBasicTypes()
    {
        basicTypes = new Dictionary<long, IType>
        {
            [1] = types["BOOLEAN"],
            [2] = types["INTEGER"],
            [4] = types["OCTET STRING"],
            [5] = types["NULL"],
            [6] = types["OBJECT IDENTIFIER"],
        };
    }

    public Dictionary<long, IType> applicationTypes = new Dictionary<long, IType>();
    public Dictionary<long, IType> privateTypes = new Dictionary<long, IType>();
    public Dictionary<long, IType> contextSpecificTypes = new Dictionary<long, IType>();

    public ASPFile(string file)
    {
        initBasicTypes();
        setParents(root);
        new Parser(this).parse(Utils.preprocessText(file));
    }

    private void setParents(ITreeNode node)
    {
        foreach(ITreeNode child in node.getChildren().Values)
        {
            child.setParent(node);
            setParents(child);
        }
    }

    public void AddType(string name, IType baseType)
    {
        if(baseType.getAddr() != null)
        {
            if (baseType.getClassId() == "APPLICATION") applicationTypes.Add(Int64.Parse(baseType.getAddr()), baseType);
            else if (baseType.getClassId() == "PRIVATE") privateTypes.Add(Int64.Parse(baseType.getAddr()), baseType);
            else contextSpecificTypes.Add(Int64.Parse(baseType.getAddr()), baseType);
        }
        types.Add(name, baseType);
    }

    public bool tryFetchType(string value, out IType type)
    {
        return types.TryGetValue(value, out type);
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

    public IType fetchType(string value)
    {
        return types[value];
    }
    long getLen(string value)
    {
        if(value[0] == '0')
        {
            var len = value.Substring(1, 7);
            return Convert.ToInt32(len, 2);
        }
        else if (Convert.ToInt32(value.Substring(1, 7), 2) != 0) // EOC type file
        {
            var octetCount = Convert.ToInt32(value.Substring(1, 7), 2);
            var longLen = value.Substring(8, 8 * octetCount);
            return Convert.ToInt64(longLen, 2);
        }

        return -1;
    }
    public (IType,long) decodeType(string value)
    {
        var type = value.Substring(0, 8);
        var type_num = Convert.ToInt32(type, 2);
        if(type_num < 31)
        {
            var type_toRet = basicTypes[type_num];
            var restOfTheString = value.Remove(0, 8);
            var len = getLen(restOfTheString);
            return (type_toRet, len);
        }
        else
        {
            var tagID = value.Substring(4, 5);
            var cla55 = value.Substring(0, 2);
            if (tagID != "11111")
            {
                if (cla55 == "01")
                {
                    var type_toRet = applicationTypes[type_num];
                    var restOfTheString = value.Remove(0, 8);
                    var len = getLen(restOfTheString);
                    return (type_toRet, len);
                }
                if (cla55 == "10")
                {
                    var type_toRet = contextSpecificTypes[type_num];
                    var restOfTheString = value.Remove(0, 8);
                    var len = getLen(restOfTheString);
                    return (type_toRet, len);
                }
                if (cla55 == "11")
                {
                    var type_toRet = privateTypes[type_num];
                    var restOfTheString = value.Remove(0, 8);
                    var len = getLen(restOfTheString);
                    return (type_toRet, len);
                }
            }
            else
            {
                var restOfTheString = value.Remove(0, 8);
                var longType = "";
                while (restOfTheString[0] != '0')
                {
                    longType = longType + restOfTheString.Substring(1, 7);
                    restOfTheString = restOfTheString.Remove(0, 8);
                }
                longType = longType + restOfTheString.Substring(1, 7);

                if (cla55 == "01")
                {
                    var type_toRet = applicationTypes[Convert.ToInt64(longType, 2)];
                    restOfTheString = value.Remove(0, 8);
                    var len = getLen(restOfTheString);
                    return (type_toRet, len);
                }
                if (cla55 == "10")
                {
                    var type_toRet = contextSpecificTypes[Convert.ToInt64(longType, 2)];
                    restOfTheString = value.Remove(0, 8);
                    var len = getLen(restOfTheString);
                    return (type_toRet, len);
                }
                if (cla55 == "11")
                {
                    var type_toRet = privateTypes[Convert.ToInt64(longType, 2)];
                    restOfTheString = value.Remove(0, 8);
                    var len = getLen(restOfTheString);
                    return (type_toRet, len);
                }
            }            
         }        
        return (null,0);
    }
}
