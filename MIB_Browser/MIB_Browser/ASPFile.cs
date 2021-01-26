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

    public Dictionary<String, IType> types = new Dictionary<String, IType>
    {
        ["INTEGER"] = new IntegerType("INTEGER", Int64.MinValue, Int64.MaxValue, null, null, null),
        ["OCTET STRING"] = new StringType("OCTET STRING", 0, Int64.MaxValue, null, null, null),
        ["OBJECT IDENTIFIER"] = new OIDType("OBJECT IDENTIFIER", Int64.MinValue, Int64.MaxValue, null, null, null),
        ["NULL"] = new NullType("NULL", null, null, null),
        ["BOOLEAN"] = new BoolType("BOOLEAN", null, null, null),
        ["SEQUENCE"] = new SequenceType("SEQUENCE", null, null, null, null)
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
            [16] = types["SEQUENCE"]
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

    public void AddType(IType baseType)
    {
        if(baseType.addr != null)
        {
            if (baseType.classId == "APPLICATION") applicationTypes.Add(Int64.Parse(baseType.addr), baseType);
            else if (baseType.classId == "PRIVATE") privateTypes.Add(Int64.Parse(baseType.addr), baseType);
            else contextSpecificTypes.Add(Int64.Parse(baseType.addr), baseType);
        }
        types.Add(baseType.name, baseType);
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
    (long,string) getLen(string value)
    {
        if(value.Substring(0, 8) == "10000000")
        {
            var restOfTheString = value.Substring(8);
            restOfTheString = restOfTheString.Substring(0, restOfTheString.IndexOf("0000000000000000"));
            return (restOfTheString.Length/8, restOfTheString);
        }
        else
        {
            var len = "";
            var restOfTheString = value;
            while (restOfTheString[0] != '0')
            {
                len += restOfTheString.Substring(1, 7);
                restOfTheString = restOfTheString.Substring(8);
            }
            len += restOfTheString.Substring(1, 7);
            restOfTheString = restOfTheString.Substring(8);

            return (Convert.ToInt64(len, 2), restOfTheString);
        }
    }
    public (IType, long, string) decodeType(string value)
    {
        var type = value.Substring(3, 5);
        var type_num = Convert.ToInt64(type, 2);
        var cla55 = value.Substring(0, 2);
        var restOfTheString = value.Substring(8);
        if (cla55 == "00")
        {
            var len = getLen(restOfTheString);
            var type_toRet = basicTypes[type_num];
            return (type_toRet, len.Item1, len.Item2);
        }
        else
        {
            if (type != "11111")
            {
                var len = getLen(restOfTheString);
                IType retType;
                if (cla55 == "01") retType = applicationTypes[type_num];
                else if (cla55 == "10") retType = contextSpecificTypes[type_num];
                else retType = privateTypes[type_num];
                return (retType, len.Item1, len.Item2);
            }
            else
            {
                var longType = "";
                while (restOfTheString[0] != '0')
                {
                    longType = longType + restOfTheString.Substring(1, 7);
                    restOfTheString = restOfTheString.Substring(8);
                }
                longType = longType + restOfTheString.Substring(1, 7);
                restOfTheString = restOfTheString.Substring(8);
                type_num = Convert.ToInt64(longType, 2);

                var len = getLen(restOfTheString);
                IType retType;
                if (cla55 == "01") retType = applicationTypes[type_num];
                else if (cla55 == "10") retType = contextSpecificTypes[type_num];
                else retType = privateTypes[type_num];
                return (retType, len.Item1, len.Item2);
            }            
        }        
    }
}
