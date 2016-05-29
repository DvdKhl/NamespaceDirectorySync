using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NamespaceDirectorySync {
	class Program {
		static void Main(string[] args) {
#if DEBUG
			args = new string[] { Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).FullName).FullName).FullName };
			args = new string[] { @"C:\Users\DvdKhl\Source\Repos\AVDump3" };
#endif

			if(args.Length != 1 || !Directory.Exists(args[0])) {
				Console.WriteLine(args.Length == 1 ? "Directory not found" : "Argumentcount needs to be 1");
				return;
			}

			var basePath = args[0];
			if(basePath.EndsWith(Path.DirectorySeparatorChar.ToString()) || basePath.EndsWith(Path.AltDirectorySeparatorChar.ToString())) {
				basePath = basePath.Substring(0, basePath.Length - 1);
			}

			foreach(var filePath in Directory.EnumerateFiles(basePath, "*.cs", SearchOption.AllDirectories)) {
				var fileContent = File.ReadAllText(filePath);

				AdhocWorkspace workspace = new AdhocWorkspace();
				Project project = workspace.AddProject(nameof(NamespaceDirectorySync), LanguageNames.CSharp);
				Document document = project.AddDocument(Path.GetFileName(filePath), SourceText.From(fileContent));
				var tree = document.GetSyntaxTreeAsync().Result;

				var namespaces = tree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>().ToArray();

				if(namespaces.Length != 1) {
					continue;
				}

				var destNamespace = Path.GetDirectoryName(filePath).Substring(basePath.Length)
					.Replace(Path.DirectorySeparatorChar, '.').Replace(Path.AltDirectorySeparatorChar, '.');

				if(destNamespace.StartsWith(".")) {
					destNamespace = destNamespace.Substring(1);
				}

				fileContent = fileContent.Remove(namespaces[0].Name.SpanStart, namespaces[0].Name.Span.Length).Insert(namespaces[0].Name.SpanStart, destNamespace);

				Console.WriteLine(filePath);
				File.WriteAllText(filePath, fileContent);
			}

		}
	}
}
