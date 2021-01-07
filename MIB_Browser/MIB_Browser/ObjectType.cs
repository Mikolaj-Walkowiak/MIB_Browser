using System;
using System.Collections.Generic;

public class ObjectType : ITreeNode
{
	public IType type;
	public string access;
	public string status;
	public string description;
    public string value;

    public int id;
    public string name;
	public ITreeNode parent;
	public Dictionary<int, ITreeNode> children = new Dictionary<int, ITreeNode>();
    public int getId()
    {
        return id;
    }
    public string getName()
    {
        return name;
    }
    public string getValue()
    {
        return value;
    }
    public Dictionary<int, ITreeNode> getChildren()
    {
        return children;
    }

    public ITreeNode getParent()
    {
        return parent;
    }
    public void setParent(ITreeNode parent)
    {
        this.parent = parent;
    }
    public void addChild(int id, ITreeNode child)
    {
        children.Add(id, child);
    }
}
