using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Linq;

namespace docs
{
	public class Vault
	{
		private readonly string root;
		
		private string documentsDirectory{
			get{
				return root + @"/documents";
			}
		}

		private string scansDirectory{
			get{
				return documentsDirectory + @"/scans";
			}
		}

		private string transcriptsDirectory
		{
			get{ 
				return documentsDirectory + @"/transcripts"; 
			}
		}

		private Vault(string directory){
			if (directory.EndsWith("/")) directory = directory.Remove(directory.Length-1,1);
			this.root = directory;
		}

		public string AddDocument(string[] transcripts, string pdfFilePath, string[] tags, string[] aliases)
		{
			string id = GenerateId();

			//add the scanned pdf file
			File.Copy(pdfFilePath, GetScanFilePath(id));

			//add all pages of the transcript each in a seperate file
			for (int i = 0; i < transcripts.Length; i++){
				string transcript = File.ReadAllText(transcripts[i]);
				File.WriteAllText(GetTranscriptFilePath(id,i+1), GenerateTranscriptFileContent(transcript, id, i+1, transcripts.Length));
			}

			//add the document-node
			File.WriteAllText(GetDocumentFilePath(id), GenerateDocumentFileContent(id,transcripts.Length, tags, aliases));
			return id;
		}

		public struct Document
		{
			public readonly string id;
			public readonly string scanFilePath;
			public readonly string[] transcripts;
			
			internal Document(string id, string scanFilePath, string[] transcripts){
				this.id = id;
				this.scanFilePath = scanFilePath;
				this.transcripts = transcripts;
			}
		}

		public Document GetDocument(string id){
			string scanFilePath = GetScanFilePath(id);
			List<string> transcripts = new List<string>();

			int n = 1;
			string transcriptFile = GetTranscriptFilePath(id, n);
			while(File.Exists(transcriptFile)){
				string[] lines = File.ReadAllText(transcriptFile).Split("\r\n");
				transcripts.Add(ConcatStringsWithSeperator(lines.Skip(9).ToArray(), "\n"));
				n++;
				transcriptFile = GetTranscriptFilePath(id, n);
			}

			return new Document(id, scanFilePath, transcripts.ToArray());
		}

		public void DropDocument(string id)
		{
			int n = 1;
			string transcriptFileName = GetTranscriptFilePath(id, n);
			while (File.Exists(transcriptFileName))
			{
				File.Delete(transcriptFileName);
				n++;
				transcriptFileName = GetTranscriptFilePath(id, n);
			}

			File.Delete(GetDocumentFilePath(id));
			File.Delete(GetScanFilePath(id));
		}


		string GetTranscriptFilePath(string id, int page){
			return transcriptsDirectory + $"/{GetTranscriptName(id, page)}.md";
		}

		string GetDocumentFilePath(string id){
			return documentsDirectory + $"/{GetDocumentName(id)}.md";
		}

		string GetScanFilePath(string id){
			return scansDirectory + $"/{GetScanName(id)}.pdf";
		}

		static string GetTranscriptName(string id, int page){
			return $"T-{id}-S{page}";
		}

		static string GetDocumentName(string id){
			return $"D-{id}";
		}

		static string GetScanName(string id){
			return $"F-{id}";
		}

		static string GenerateTranscriptFileContent(string transcript, string id, int n, int l)
		{
			string result = "";
			result += "---\n";
			result += "tags: type/transcript\n";
			result += "---\n";
			result += $"📁 -> [[{GetDocumentName(id)}]]\n";
			result += $"📄 -> [[{GetScanName(id)}.pdf]]\n";
			result += "\n";
			result += $"Page {n} of {l} \n";
			result += "\n";
			result += "---\n";
			result += transcript;
			return result;
		}

		static string GenerateDocumentFileContent(string id, int n, string[] customTags, string[] aliases)
		{
			string result = "";
			result += "---\n";
			result += $"aliases: {ConcatStringsWithSeperator(aliases, " ")}\n";
			result += $"tags: type/document {ConcatStringsWithSeperator(customTags, " ")}\n";
			result += "---\n";
			result += $"# 📁 *{id}*\n";
			result += "\n";
			result += $"📑 -> [[{GetScanName(id)}.pdf]]\n";
			for(int i = 0; i < n; i++){
				result += $"{i+1}. 📜 -> [[{GetTranscriptName(id, i+1)}]]";
				if(i < n-1){
					result += "\n";
				}
			}
			return result;
		}

		static string ConcatStringsWithSeperator(string[] arr, string sep)
		{
			if (arr == null)
				return "";

			string result = "";
			for (int i = 0; i < arr.Length; i++)
			{
				result += arr[i];
				if (i < arr.Length - 1)
				{
					result += sep;
				}
			}

			return result;
		}

		private static string GenerateId()
		{
			Thread.Sleep(1010);
			DateTime time = DateTime.Now;
			string result = time.Year.ToString("0000");
			result += time.Month.ToString("00");
			result += time.Day.ToString("00");
			result += time.Hour.ToString("00");
			result += time.Minute.ToString("00");
			result += time.Second.ToString("00");
			return result;
		}


		public static Vault Init(string vaultDirectory){
			if(!Directory.Exists(vaultDirectory)){
				throw new DirectoryNotFoundException();
			}

			Vault vault = new Vault(vaultDirectory);
			return vault;
		}
	}
}
