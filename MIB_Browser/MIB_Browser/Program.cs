using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MIB_Browser
{
    class Program
    {

        static void Main(string[] args)
        {
            string input = File.ReadAllText("inp.txt");
            Console.WriteLine(input);
            Match match = Expressions.fullImports.Match(input);
            Console.WriteLine(match.Value);
            
        }
    }
}
