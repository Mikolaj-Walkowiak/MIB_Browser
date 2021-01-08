﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                if (command[0] == "quit" && command.Length == 1)
                {
                    quit = true;
                }
                else if (command[0] == "start" && command.Length == 1)
                {
                    Console.WriteLine("parsing file RFC1213 - MIB.txt");
                    file = new ASPFile(File.ReadAllText("RFC1213-MIB.txt"));
                    node = file.root;
                }
                else if (command[0] == "test" && command.Length == 1)
                {
                    var str = new OIDType(Int64.MinValue, Int64.MaxValue, null, null, null).encode("{ISO(5)}");
                    string hex = String.Concat(
                      Regex.Matches(str, "....").Cast<Match>()
                      .Select(m => Convert.ToInt32(m.Value, 2)
                      .ToString("x1"))
                    );
                    Console.WriteLine(str);
                    Console.WriteLine(hex);
                }
                else if (command[0] == "cls" && command.Length == 1)
                {
                    Console.Clear();
                }
                else if (command[0] == "parse" && command.Length == 2)
                {
                    Console.WriteLine("parsing file " + command[1]);
                    file = new ASPFile(File.ReadAllText(command[1]));
                    node = file.root;
                }
                else if (command[0] == "cd" && command.Length == 2)
                {
                    ITreeNode newNode = file.findPath(command[1].Split('.'), node);
                    if (newNode != null) node = newNode;
                    else Console.WriteLine("node not found");
                }
                else if (command[0] == "ls" && command.Length == 1)
                {
                    printChildren(node);
                }
                else if (command[0] == "discard" && command.Length == 1)
                {
                    file = null;
                    node = null;
                }
                else if (command[0] == "find" && command.Length == 2)
                {
                    ITreeNode newNode = file.findNode(command[1]);
                    if (newNode != null) node = newNode;
                    else Console.WriteLine("node not found");
                }
                else if (command[0] == "info" && command.Length == 1)
                {
                    printInfo(node);
                }
                else if (command[0] == "new" && command.Length == 1)
                {
                    file = new ASPFile("");
                    node = file.root;
                }
                else if (command[0] == "def" && command.Length > 1)
                {
                    string line = string.Join(' ', command.AsSpan(1, command.Length - 1).ToArray());
                    new Parser(file).parseAnyType(Utils.preprocessText(line + ' '));
                }
                else if (command[0] == "encode" && command.Length > 2)
                {
                    string value = string.Join(' ', command.AsSpan(2, command.Length - 2).ToArray());
                    Console.WriteLine(file.fetchType(command[1]).encode(value));
                }
                else Console.WriteLine("unknown command");
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

        private static void printInfo(ITreeNode node)
        {
            if(node.GetType() == typeof(ObjectId))
            {
                Console.WriteLine(node.getName() + "(" + node.getId() +  "): OBJECT IDENTIFIER");
                Console.WriteLine("path: " + String.Join('.', getPath(node)));
                Console.WriteLine(node.getChildren().Count == 1 ? "1 child" : node.getChildren().Count + " children");
            }
            else if(node.GetType() == typeof(ObjectType))
            {
                ObjectType objType = (ObjectType)node;
                Console.WriteLine(node.getName() + "(" + node.getId() + "): OBJECT TYPE");
                Console.WriteLine("path: " + String.Join('.', getPath(node)));
                Console.WriteLine("access: " + objType.access);
                Console.WriteLine("status: " + objType.status);
                Console.WriteLine("value: " + objType.value);
                Console.WriteLine("description: " + objType.description);
                Console.WriteLine(node.getChildren().Count == 1 ? "1 child" : node.getChildren().Count + " children");
            }
        }
    }
}
