using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MIB_Browser
{
    class Program
    {
        public static ASPFile file = null;
        public static Boolean quit = false;
        public static ITreeNode node = null;
        static void Main(string[] args)
        {
            while(!quit)
            {
                if (node == null) Console.Write(">");
                else Console.Write(String.Join('.', getPath(node)) + " $ ");
                string[] command = Console.ReadLine().Split(" ");
                switch(command[0])
                {
                    case "quit":
                        quit = true;
                        break;
                    case "parse":
                        file = new ASPFile(command[1]);
                        node = file.root;
                        break;
                    case "cd":
                        ITreeNode newNode = file.findPath(command[1].Split('.'), node);
                        if (newNode != null) node = newNode;
                        else Console.WriteLine("node not found");
                        break;
                    case "ls":
                        printChildren(node);
                        break;
                    case "discard":
                        file = null;
                        node = null;
                        break;
                    default:
                        Console.WriteLine("unknown command");
                        break;
                    
                }
            }
        }

        static List<string> getPath(ITreeNode node)
        {
            List<string> path = new List<string>();
            if (node.getParent() != null)
            {
                path.AddRange(getPath(node.getParent()));
                path.Add(node.getName());
            }
            return path;
        }

        static void printChildren(ITreeNode node)
        {
            if(node.getChildren().Count == 0)
            {
                Console.WriteLine("no children");
            }
            else
            {
                foreach(ITreeNode child in node.getChildren().Values)
                {
                    Console.WriteLine(child.getName() + "(" + child.getId() + ") : " + getType(node));
                }
            }
        }

        private static string getType(ITreeNode node)
        {
            return node.GetType().ToString();
        }
    }
}
