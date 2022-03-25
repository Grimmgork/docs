using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace docs
{
	class Program
	{
		public static string directory;
		public static readonly string vaultDirectory = @"d:\document_vault";

		static void Main(string[] args)
		{
			directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			RootCommand(args);
		}

		static void RootCommand(string[] args)
		{
			if (args.Length == 0){
				ShowDash();
				return;
			}
			
			string command = args[0];
			args = args.Skip(1).ToArray();
			switch (command)
			{
				case "add":
					AddDocument(args);
					break;
				case "remove":
					Console.WriteLine("not implemented! ... yet");
					break;
				case "find":
					Console.WriteLine("not implemented! ... yet");
					break;
				default:
					Console.WriteLine($"command {command} not found!");
					break;
			}
		}

		static void ShowDash()
		{
			const string logo = "██████╗  ██████╗  ██████╗███████╗\n" +
								"██╔══██╗██╔═══██╗██╔════╝██╔════╝\n" +
								"██║  ██║██║   ██║██║     ███████╗\n" +
								"██║  ██║██║   ██║██║     ╚════██║\n" +
								"██████╔╝╚██████╔╝╚██████╗███████║\n" +
								"╚═════╝  ╚═════╝  ╚═════╝╚══════╝";

			Console.WriteLine(logo);
			Console.WriteLine();
			Console.WriteLine("by Eric Armbruster");
			Console.WriteLine("- A system to automatically store scans of real documents in an obisidan vault.");
			Console.WriteLine();
			Console.WriteLine($"vault: {vaultDirectory}");
			Console.WriteLine();
			Console.WriteLine("functions:");
			Console.WriteLine("\tadd [path] [options]");
			Console.WriteLine("\tremove [document id]");
		}

		static void RemoveDocument(string[] args)
		{
			Arguments arguments = Arguments.Parse(args,
				new string[] { "id" },
				new string[] { },
				new string[] { },
				new string[] { },
				new string[] { },
				new string[] { }
				);

			Vault vault = Vault.Init(@"d:\document_vault");
		}

		static void AddDocument(string[] args)
		{
			Arguments arguments = Arguments.Parse(args,
				new string[] { "path" },
				new string[] { "tags", "filter", "language" },
				new string[] { "t", "f", "l" },
				new string[] { "", "*.*", "deu" },
				new string[] { },
				new string[] { }
				);

			string[] tags = arguments.GetArgument("tags").Split(",");
			string path = arguments.GetArgument("path");
			string[] files;

			if (File.Exists(path))
			{
				files = new string[] { path };
			}
			if (!Directory.Exists(path))
			{
				throw new DirectoryNotFoundException();
			}

			files = Directory.GetFiles(path, arguments.GetArgument("filter"));
			if (files.Length == 0)
			{
				Console.WriteLine("No files selected, quitting ...");
				return;
			}

			Console.WriteLine("files:");
			foreach (string f in files)
			{
				Console.WriteLine(f);
			}

			Console.WriteLine();
			Console.WriteLine("tags:");
			foreach (string t in tags)
			{
				Console.WriteLine(t);
			}

			Console.WriteLine();

			DocumentData doc = DocumentData.Aggregate(directory + @"\tessdata", files, arguments.GetArgument("language"));

			Vault vault = Vault.Init(vaultDirectory);
			Console.WriteLine($"addet document -> id={vault.AddDocument(doc.Transcripts, doc.PdfFilePath, tags, null)}");
		}
	}
}