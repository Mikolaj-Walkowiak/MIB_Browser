using System;
using System.Collections.Generic;

public class ObjectId : ITreeNode
{
    public int id { get; set; }
    public string name { get; set; }
    public ITreeNode parent { get; set; }
    public Dictionary<int, ITreeNode> children{ get; set; } = new Dictionary<int, ITreeNode>();

    public int getId()
    {
        return id;
    }
    public string getName()
    {
        return name;
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
