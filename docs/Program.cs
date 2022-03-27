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
		public static readonly Command rootCommand = new Command("root",
			new Command[] {
				new Command("add", null, AddDocumentCommand),
				new Command("remove", null, RemoveDocumentCommand),
				new Command("dash", null, ShowDashCommand)
			},
			null
		);

		static void Main(string[] args)
		{
			directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			if(args.Length == 0){
				args = new string[] { "dash" };
			}

			//try{
				rootCommand.Route(args);
			//}
			//catch(Exception e){
				//Console.WriteLine($"ERROR: {e.Message}");
			//}
		}

		static void ShowDashCommand(string[] args)
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

		static void RemoveDocumentCommand(string[] args)
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

		static void AddDocumentCommand(string[] args)
		{
			Arguments arguments = Arguments.Parse(args,
				new string[] { "path" },
				new string[] { "tags", "filter", "language", "sort" },
				new string[] { "t", "f", "l", "s" },
				new string[] { "", "*.*", "deu", "date" },
				new string[] { },
				new string[] { }
				);

			string[] tags = ParseList(arguments.GetArgument("tags"), ",");
			string path = arguments.GetArgument("path");

			string[] files = new string[] { };
			if (File.Exists(path))
			{
				files = new string[] { path };
			}
			else
			{
				if (!Directory.Exists(path)){
					throw new DirectoryNotFoundException();
				}

				files = Directory.GetFiles(path, arguments.GetArgument("filter"));
				string sortModeName = arguments.GetArgument("sort");
				switch (sortModeName)
				{
					case "date":
						files = files.Select(fn => new FileInfo(fn)).OrderBy(f => f.CreationTimeUtc).Select(fn => fn.FullName).ToArray();
						break;
					case "name":
						files = files.Select(fn => new FileInfo(fn)).OrderBy(f => f.Name).Select(fn => fn.FullName).ToArray();
						break;
					default:
						throw new Exception($"invalid sort-mode '{sortModeName}'");
				}
			}

			if (files.Length == 0){
				Console.WriteLine("No files selected, quitting ...");
				return;
			}

			Console.WriteLine("files:");
			foreach (string f in files){
				Console.WriteLine(f);
			}

			Console.WriteLine();
			Console.WriteLine("tags:");
			foreach (string t in tags){
				Console.WriteLine(t);
			}

			Console.WriteLine();
			Console.WriteLine("running ocr ...");

			DocumentData doc = DocumentData.Aggregate(directory + @"\tessdata", files, arguments.GetArgument("language"));
			Console.WriteLine("ocr done!");
			Console.WriteLine();

			Vault vault = Vault.Init(vaultDirectory);
			Console.WriteLine($"addet document to vault -> id={vault.AddDocument(doc.Transcripts, doc.PdfFilePath, tags, null)}");
		}


		static string[] ParseList(string s, string delim){
			string[] split = s.Split(delim);
			List<string> result = new List<string>();

			foreach(string e in split){
				if (e == null | e == string.Empty)
					continue;
				result.Add(e);
			}

			return result.ToArray();
		}
	}
}