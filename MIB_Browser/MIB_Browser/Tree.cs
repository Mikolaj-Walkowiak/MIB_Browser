using System;
using System.Collections.Generic;

public class Class1
{

	struct Node {
		int NodeNumber { get; }
		string NodeName { get; }

		string Syntax { get; }
		string Access { get; }
		string Status { get; }
		string Description { get; }

		public Node(int nodeNumber, string nodeName, string syntax, string access, string status, string description)
		{
			NodeNumber = nodeNumber;
			NodeName = nodeName;
			Syntax = syntax;
			Access = access;
			Status = status;
			Description = description;
		}
	}
	struct Tree{
		Node root { get; }
		public List<Node> children;




        Tree()
        {

			children = new List<Node>();

		}

	
	}
	
}
