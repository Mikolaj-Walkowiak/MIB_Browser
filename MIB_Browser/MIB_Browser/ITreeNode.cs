using System;
using System.Collections.Generic;

public interface ITreeNode
{
    int getId();
	string getName();
	Dictionary<int, ITreeNode> getChildren();
	ITreeNode getParent();
    void setParent(ITreeNode parent);

	ITreeNode getChildByName(string name)
    {
        if (getChildren() == null) return null;
        if (Int32.TryParse(name, out int i))
        {
            return getChildByIndex(i);
        } else if (Expressions.Path.mixed.IsMatch(name))
        {
            string index = Expressions.Path.mixed.Match(name).Groups[2].Value;
            return getChildByIndex(Int32.Parse(index));
        }
		foreach(ITreeNode child in getChildren().Values)
        {
			if(child.getName() == name)
            {
                return child;
            }
        }
        return null;
    }

    ITreeNode getChildByIndex(int i) => getChildren().GetValueOrDefault(i, null);

    void addChild(int id, ITreeNode child);
}
