using System;
using System.Collections.Generic;
using System.Text;
using Tesseract;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace docs
{
	public class DocumentData
	{
		string _pdf;
		public string PdfFilePath{
			get{
				return _pdf;
			}
			private set{
				_pdf = value;
			}
		}

		string[] _transcripts;
		public string[] Transcripts{
			get{
				return _transcripts;
			}
			private set{
				_transcripts = value;
			}
		}

		private DocumentData() { }

		public static DocumentData Aggregate(string tesseractData, string[] sourceImagesPaths, string languageCode)
		{
			string pdfFileNameWithoutPdfExtension = Path.GetTempFileName();

			DocumentData result = new DocumentData();
			result.PdfFilePath = pdfFileNameWithoutPdfExtension + ".pdf";
			result.Transcripts = new string[sourceImagesPaths.Length];

			Task<PageRender>[] tasks = new Task<PageRender>[sourceImagesPaths.Length];
			for (int i = 0; i < sourceImagesPaths.Length; i++)
			{
				tasks[i] = Task.Factory.StartNew((index) =>
				{
					Console.WriteLine($"Start task #{index}");
					PageRender render = PageRender.Render(sourceImagesPaths[(int)index], new TesseractEngine(tesseractData, languageCode, EngineMode.Default));
					result.Transcripts[(int)index] = FormatTranscript(render.GetPage().GetText(), 80);
					Console.WriteLine($"Task #{index} done!");
					return render;
				}, i);
			}

			Task.WaitAll(tasks);
			Console.WriteLine("All tasks done!");

			using (var renderer = PdfResultRenderer.CreatePdfRenderer(pdfFileNameWithoutPdfExtension, tesseractData, false))
			{
				renderer.BeginDocument("-");
				foreach(Task<PageRender> t in tasks){
					renderer.AddPage(t.Result.GetPage());
				}
			}

			Console.WriteLine("PDF generated!");

			foreach (Task<PageRender> t in tasks){
				t.Result.Dispose();
			}

			return result;
		}

		public static string FormatTranscript(string raw, int maxLineLength)
		{
			raw = raw.Trim();
			raw = raw.Replace("\n", " ");
			raw = Regex.Replace(raw, @"[ ]{2,}", @" ", RegexOptions.None);

			return raw;
		}

		public class PageRender : IDisposable
		{
			private TesseractEngine engine;
			private Page page;

			private PageRender(TesseractEngine engine, Page page)
			{
				this.page = page;
				this.engine = engine;
			}

			public static PageRender Render(string imagePath, TesseractEngine engine)
			{
				var image = Pix.LoadFromFile(imagePath);
				return new PageRender(engine, engine.Process(image));
			}

			public Page GetPage(){
				return page;
			}

			public void Dispose()
			{
				engine.Dispose();
			}
		}
	}
}
