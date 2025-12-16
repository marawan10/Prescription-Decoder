using Newtonsoft.Json;
using PrescriptionDecoder.Domain.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Text.RegularExpressions;
using PrescriptionDecoder.Application.Interfaces;

using Microsoft.Extensions.Configuration;

namespace PrescriptionDecoder.Infrastructure.Services
{
    public class OcrSpaceService : IOcrSpaceService
    {
        private readonly string _apiKey; // User Provided Key
        private readonly string _endpoint = "https://api.ocr.space/parse/image";
        private readonly IImagePreprocessingService _preprocessingService;

        public OcrSpaceService(IImagePreprocessingService preprocessingService, IConfiguration configuration)
        {
            _preprocessingService = preprocessingService;
            _apiKey = configuration["ApiKeys:OcrSpace"];
        }

        public async Task<List<Medicine>> AnalyzePrescriptionAsync(Stream imageStream)
        {
            try 
            {
                string rawText = await GetRawTextAsync(imageStream);
                // 4. Extract Medicines using Heuristics (Regex)
                return ParseMedicinesFromText(rawText);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OCR Service Error: {ex.Message}");
                return new List<Medicine>(); // Return empty if OCR fails, don't crash
            }
        }

        public async Task<string> GetRawTextAsync(Stream imageStream)
        {
            // 1. Preprocess Image (Black & White + High Contrast)
            using var processedImageStream = _preprocessingService.PreprocessForOcr(imageStream);

            // 2. Send to OCR.Space
            using var client = new HttpClient();
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(_apiKey), "apikey");
            content.Add(new StringContent("eng"), "language"); 
            content.Add(new StringContent("true"), "isOverlayRequired"); 
            content.Add(new StringContent("2"), "OCREngine"); 
            content.Add(new StreamContent(processedImageStream), "file", "prescription.jpg");

            var response = await client.PostAsync(_endpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) return "";

            // 3. Parse JSON Response
            dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
            
            if (jsonResponse.IsErroredOnProcessing == true) return "";

            string parsedText = "";
            var parsedResults = jsonResponse.ParsedResults;
            if (parsedResults != null && parsedResults.Count > 0)
            {
                parsedText = parsedResults[0].ParsedText;
            }
            return parsedText;
        }



        private List<Medicine> ParseMedicinesFromText(string text)
        {
            var medicines = new List<Medicine>();
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var cleanLine = line.Trim();
                if (string.IsNullOrWhiteSpace(cleanLine)) continue;
                if (cleanLine.Length < 3) continue; // Skip noise

                // Heuristic Helpers
                // Match Dose: 500mg, 1g, 5ml, etc.
                var doseMatch = Regex.Match(cleanLine, @"(\d+(\.\d+)?\s?(mg|g|ml|mcg|unit|Tablet|Cap))", RegexOptions.IgnoreCase);
                string dose = doseMatch.Success ? doseMatch.Value : "Not Stated";

                // Match Freq: 1x3, 1 tab, twice daily, every 8 hours
                var freqMatch = Regex.Match(cleanLine, @"(\d+x\d+|twice|thrice|daily|every \d+ h|q\d+h|bedtime)", RegexOptions.IgnoreCase);
                string freq = freqMatch.Success ? freqMatch.Value : "Not Stated";

                // Assume whatever is NOT dose/freq is potentially the drug name
                // This is a naive heuristic but works for simple lines like "Panadol 500mg 1x3"
                string drug = cleanLine;
                if (dose != "Not Stated") drug = drug.Replace(dose, "").Trim();
                if (freq != "Not Stated") drug = drug.Replace(freq, "").Trim();
                
                // Cleanup drug name symbols
                drug = Regex.Replace(drug, @"[^\w\s-]", "");

                if (!string.IsNullOrWhiteSpace(drug))
                {
                    medicines.Add(new Medicine
                    {
                        Drug = drug,
                        Dose = dose,
                        Freq = freq,
                        Notes = $"Extracted via OCR from line: '{cleanLine}'"
                    });
                }
            }

            return medicines;
        }
    }
}
