/*
    Falak compiler 
    Copyright (C) 2021 José Antonio Vázquez, Daniel Trejo y Jaime Orlando López. ITESM CEM
*/


using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Falak
{

    public class Driver
    {

        const string VERSION = "4.0.0";


        static readonly string[] ReleaseIncludes = {
            "Lexical analysis",
            "Syntatic analysis",
            "AST Construction",
            "Semantic analysis"
        };


        void PrintAppHeader()
        {
            Console.WriteLine("Falak compiler, version " + VERSION);
            Console.WriteLine(
                "Copyright \u00A9 2021, ITESM CEM.");
            Console.WriteLine("This program is free software; you may "
                + "redistribute it under the terms of");
            Console.WriteLine("the GNU General Public License version 3 or "
                + "later.");
            Console.WriteLine("This program has absolutely no warranty.");
        }


        void PrintReleaseIncludes()
        {
            Console.WriteLine("Included in this release:");
            foreach (var phase in ReleaseIncludes)
            {
                Console.WriteLine("   * " + phase);
            }
        }


        void Run(string[] args)
        {

            PrintAppHeader();
            Console.WriteLine();
            PrintReleaseIncludes();
            Console.WriteLine();

            if (args.Length != 1)
            {
                Console.Error.WriteLine(
                    "Please specify the name of the input file.");
                Environment.Exit(1);
            }

            try
            {
                var inputPath = args[0];
                var input = File.ReadAllText(inputPath);
                var parser = new Parser(
                    new Scanner(input).Scan().GetEnumerator());
                var program = parser.Program();
                Console.WriteLine("Syntax is OKEY");

                var semantic = new SemanticVisitor();
                semantic.Visit((dynamic)program);

                Console.WriteLine("Semantics is OKEY");
                Console.WriteLine();
                Console.WriteLine("Function Table");
                Console.WriteLine("-*-*-*-*-*-*-*-*-*-*");
                foreach (var entry in semantic.GlobalFunctionTable)
                {
                    Console.WriteLine();
                    Console.WriteLine(entry.Key);
                    if (entry.Value.getLocalTable().Any())
                    {

                        Console.WriteLine("Local Table");
                        Console.WriteLine("-*-*-*-*-*-*-*-*-*-*");

                        foreach (var variable in entry.Value.getLocalTable())
                        {
                            Console.WriteLine(variable);
                        }
                        Console.WriteLine();
                        
                    }                   
                }

                Console.WriteLine();
                Console.WriteLine("Global Variable Table");
                Console.WriteLine("-*-*-*-*-*-*-*-*-*-*");
                foreach (var entry in semantic.GlobalVariableTable)
                {
                    Console.WriteLine(entry);
                }


            }
            catch (Exception e)
            {

                if (e is FileNotFoundException || e is SyntaxError || e is SemanticError)
                {
                    Console.Error.WriteLine(e.Message);
                    Environment.Exit(1);
                }

                throw;
            }

        }


        public static void Main(string[] args)
        {
            new Driver().Run(args);
        }
    }
}
