using System;
using System.IO;
using System.Threading;

namespace docs
{
	class Program
	{
		static void Main(string[] args)
		{
			const string vaultDirectory = @"D:\document_vault\document_vault";
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

				Console.WriteLine();
				Console.WriteLine("digesting! ...");

				DocumentData doc;
				try
				{
					doc = DocumentData.Extract(files, "deu");
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					return;
				}

				Vault vault = Vault.Init(vaultDirectory);
				vault.AddDocument(doc.GetTranscripts(), doc.Pdf, null, null);

				foreach (string s in doc.GetTranscripts()){
					File.Delete(s);
				}
				File.Delete(doc.Pdf);

				foreach(string s in files){
					File.Delete(s);
				}

				Console.WriteLine("Done! ~");
				Console.WriteLine();
				Thread.Sleep(500);
			}
		}
	}
}