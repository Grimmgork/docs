using System;
using System.Collections.Generic;
using System.Text;
using Tesseract;
using System.IO;
using System.Text.RegularExpressions;

namespace docs
{
	public class DocumentData
	{
		string _pdf;
		public string Pdf{
			get{
				return _pdf;
			}
			private set{
				_pdf = value;
			}
		}

		string[] transcriptFileNames;

		public string[] GetTranscripts(){
			return (string[]) transcriptFileNames.Clone();
		}

		private DocumentData() { }

		public static DocumentData Extract(string[] sourceImagesPaths, string languageCode)
		{
			DocumentData result = new DocumentData();
			string pdfFileNameWithoutPdfExtension = Path.GetTempFileName();
			result.Pdf = pdfFileNameWithoutPdfExtension + ".pdf";
			result.transcriptFileNames = new string[sourceImagesPaths.Length];

			using (var engine = new TesseractEngine(@"./tessdata", languageCode, EngineMode.Default))
			{
				using (var renderer = PdfResultRenderer.CreatePdfRenderer(pdfFileNameWithoutPdfExtension, @"./tessdata", false))
				{
					renderer.BeginDocument("-");
					int pageNumber = 1;
					foreach(string imagePath in sourceImagesPaths){
						using (var img = Pix.LoadFromFile(imagePath))
						{
							using (var page = engine.Process(img))
							{
								renderer.AddPage(page);

								string fileName = Path.GetTempFileName();
								result.transcriptFileNames[pageNumber - 1] = fileName;
								File.WriteAllText(fileName, FormatTranscript(page.GetText(), 80));
							}
						}
						pageNumber++;
					}
				}	
			}

			return result;
		}

		public static string FormatTranscript(string raw, int maxLineLength){

			raw = raw.Trim();
			raw = raw.Replace("\r\n", " ");
			raw = Regex.Replace(raw, @"[ ]{2,}", @" ", RegexOptions.None);

			return raw;
		}
	}
}
