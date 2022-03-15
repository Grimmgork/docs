using System;
using System.IO;
using System.Threading;

namespace docs
{
	class Program
	{
		static void Main(string[] args)
		{
			const string vaultDirectory = @"d:\document_vault";
			const string funnel = @"c:\users\eric\desktop\doc.gateway";

			Console.WriteLine("Hello there!");
			while (true){

				Console.WriteLine("Press enter to aggregate a document ...");
				Console.ReadLine();

				string[] files = Directory.GetFiles(funnel);
				if(files.Length == 0){
					Console.WriteLine("ERROR: No files found!");
					Console.WriteLine();
					Thread.Sleep(100);
					continue;
				}

				Console.WriteLine($"{files.Length} files staged:");
				foreach(string f in files){
					Console.WriteLine(f);
				}
				Console.WriteLine();
				Console.WriteLine("tags:");
				string[] tags = Console.ReadLine().Trim().Split(" ");
				Console.WriteLine();
				Console.WriteLine("Adding the following tags:");
				foreach(string t in tags){
					Console.WriteLine("\t"+ t);
				}
				Console.WriteLine();

				Console.WriteLine("lesgo! ->");
				Console.WriteLine("searching for text ...");

				DocumentData doc;
				doc = DocumentData.Extract(files, "deu");

				Vault vault = Vault.Init(vaultDirectory);
				Console.WriteLine($"addet document -> id={vault.AddDocument(doc.GetTranscripts(), doc.Pdf, tags, null)}");

				Console.WriteLine("clean up temporary files ...");
				foreach (string s in doc.GetTranscripts()){
					File.Delete(s);
				}
				File.Delete(doc.Pdf);

				foreach(string s in files){
					File.Delete(s);
				}

				Console.WriteLine("-> done!");
				Console.WriteLine();
				Thread.Sleep(500);
			}
		}
	}
}