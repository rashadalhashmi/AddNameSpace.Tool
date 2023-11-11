using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AddNamespaceToClasses
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string rootDirectory, searchString, namespaceToAdd;

                GetUserInput(out rootDirectory, out searchString, out namespaceToAdd);

                SearchAndAddNamespace(rootDirectory, searchString, namespaceToAdd);

                Console.WriteLine("Operation complete.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ResetColor();
            }
        }

        public static void SearchAndAddNamespace(string rootDirectory, string searchString, string namespaceToAdd)
        {
            var syntaxTrees = GetSyntaxTreesWithSearchString(rootDirectory, searchString);

            AddNamespaceIfNeeded(syntaxTrees, searchString, namespaceToAdd);
        }

        public static List<SyntaxTree> GetSyntaxTreesWithSearchString(string rootDirectory, string searchString)
        {
            var syntaxTrees = new List<SyntaxTree>();

            string[] csFiles = Directory.GetFiles(rootDirectory, "*.cs", SearchOption.AllDirectories);

            foreach (string filePath in csFiles)
            {
                string fileContent = File.ReadAllText(filePath, System.Text.Encoding.UTF8);

                if (fileContent.Contains(searchString))
                {
                    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(fileContent);
                    syntaxTree = syntaxTree.WithFilePath(filePath);
                    syntaxTrees.Add(syntaxTree);
                }
            }
            return syntaxTrees;
        }

        public static void AddNamespaceIfNeeded(List<SyntaxTree> syntaxTrees, string searchString, string namespaceToAdd)
        {
            foreach (var syntaxTree in syntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

                foreach (var classDeclaration in classDeclarations)
                {
                    if (classDeclaration.ToString().Contains(searchString))
                    {
                        var existingNamespace = root.DescendantNodes().OfType<UsingDirectiveSyntax>().FirstOrDefault(u => u.Name.ToString() == namespaceToAdd);

                        if (existingNamespace == null)
                        {
                            var newNamespace = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($" {namespaceToAdd}"));

                            root = ((CompilationUnitSyntax)root).AddUsings(newNamespace);

                            var newSyntaxTree = syntaxTree.WithRootAndOptions(root, syntaxTree.Options);

                            string newContent = newSyntaxTree.ToString();

                            //add new line after add the new namespace
                            newContent = newContent.Replace($"using {namespaceToAdd};", $"using {namespaceToAdd};" + Environment.NewLine);


                            File.WriteAllText(newSyntaxTree.FilePath, newContent, Encoding.UTF8);
                            Console.WriteLine($"Added namespace to: {newSyntaxTree.FilePath}");

                        }
                    }
                }
            }

        }

        public static void GetUserInput(out string rootDirectory, out string searchString, out string namespaceToAdd)
        {
            Console.Write("Enter the root directory path: ");
            rootDirectory =  Console.ReadLine().Trim();

            Console.Write("Enter the search string: ");
            searchString =   Console.ReadLine().Trim();

            Console.Write("Enter the namespace to add: ");
            namespaceToAdd = Console.ReadLine().Trim();
        }
    }
}
