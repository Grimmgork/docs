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
			DocumentData result = new DocumentData();
			string pdfFileNameWithoutPdfExtension = Path.GetTempFileName();
			result.PdfFilePath = pdfFileNameWithoutPdfExtension + ".pdf";
			result.Transcripts = new string[sourceImagesPaths.Length];

			using (var engine = new TesseractEngine(tesseractData, languageCode, EngineMode.Default))
			{
				using (var renderer = PdfResultRenderer.CreatePdfRenderer(pdfFileNameWithoutPdfExtension, tesseractData, false))
				{
					renderer.BeginDocument("-");
					int pageNumber = 1;
					foreach(string imagePath in sourceImagesPaths){
						using (var img = Pix.LoadFromFile(imagePath))
						{
							using (var page = engine.Process(img))
							{
								renderer.AddPage(page);
								string pageContent = page.GetText();
								result.Transcripts[pageNumber - 1] = FormatTranscript(pageContent, 80);
							}
						}
						pageNumber++;
					}
				}	
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
	}
}
