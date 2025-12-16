using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using PrescriptionDecoder.Application.Interfaces;
using PrescriptionDecoder.Domain.Entities;

using Microsoft.Extensions.Configuration;

namespace PrescriptionDecoder.Infrastructure.Services
{
    public class GroqService : IGroqService
    {
        private readonly string _apiKey;
        private readonly string _endpoint = "https://api.groq.com/openai/v1/chat/completions";
        private readonly IOcrSpaceService _ocrService;
        private readonly IImagePreprocessingService _preprocessingService;

        public GroqService(IOcrSpaceService ocrService, IImagePreprocessingService preprocessingService, IConfiguration configuration)
        {
            _ocrService = ocrService;
            _preprocessingService = preprocessingService;
            _apiKey = configuration["ApiKeys:Groq"];
        }

        public async Task<Prescription> AnalyzePrescriptionImageAsync(Stream imageStream)
        {
            // 1. Clone Stream for OCR (multimodal service needs fresh stream)
            var msForOcr = new MemoryStream();
            await imageStream.CopyToAsync(msForOcr);
            msForOcr.Position = 0;
            imageStream.Position = 0; // Reset original

            // 2. Get OCR Hint
            string ocrHint = await _ocrService.GetRawTextAsync(msForOcr);
            
            Console.WriteLine($"Hybrid Hint: {ocrHint}");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            // 3. Convert Image to Base64 (Use Preprocessed Image)
            using var processedStream = _preprocessingService.PreprocessForAi(imageStream);
            using var memoryStream = new MemoryStream();
            await processedStream.CopyToAsync(memoryStream); 
            byte[] imageBytes = memoryStream.ToArray();
            string base64Image = Convert.ToBase64String(imageBytes);

            // 4. Hybrid Prompt with OCR Hint & Doctor Extraction
            var prompt = $@"
                Act as an Expert Clinical Pharmacist with 20 years of experience deciphering messy handwriting.
                Your task is to decode this handwritten prescription with 100% accuracy, specifically focusing on overlapping or touching words.

                *** VISUAL DECONSTRUCTION STRATEGY ***
                1.  **Scanning**: Scan the image line by line. Note where ink strokes from the line above might overlap.
                2.  **Letter Isolation**: For every unclear word, list the visible letters (e.g., ""I see 'A', 'u', 'g'... then a squiggle... then 'n'"").
                3.  **Specialist Context**: IDENTIFY the Doctor/Specialist FIRST. (e.g., If 'Dentist', expect dental drugs).
                4.  **Hypothesis Generation**: 
                    - For each extracted line, generate 3 candidates based on the visible letters + Specialist.
                    - Example: ""Letters 'Co...or', Specialist 'Cardio' -> Candidate: 'Concor'"".
                
                *** HYBRID HINT ***
                OCR TEXT: 
                ""{ocrHint}""

                *** KNOWLEDGE BASE: COMMON EGYPTIAN DRUGS ***
                (Use this list to correct spelling mistakes or fuzzy text)
                - Antibiotics: Augmentin, Hibiotic, Curam, Megamox, Flumox, Klacid, Zithromax, Ciprofar, Tavanic, Dalacin, Flagyl, Amikin.
                - Pain/NSAIDs: Panadol, Abimol, Cetal, Cataflam, Voltaren, Brufen, Ketolgin, Oflam, Dimra, Myolgin.
                - GI/Stomach: Antinal, Streptoquin, Visceralgine, Spasmo-Digestin, Gast-Reg, Nexium, Controloc, Downoprazol, Gaviscon, Maalox.
                
                *** OUTPUT FORMAT ***
                First, Provide a detailed 'Chain of Thought' block. Structure it like this:
                ""PHASE 1: VISUAL SCAN
                 - Line 1: ...
                 - Line 2: ... (Note overlaps)
                 PHASE 2: SPECIALIST HYPOTHESIS
                 - Doctor: ... Specialist: ...
                 PHASE 3: CANDIDATE SCORING
                 - Drug 1: Visible 'P..n..l' -> Candidates: Panadol (90%), Pantoloc (10%). Match: Panadol.""

                Then, output the final result in a JSON block with this EXACT schema:
                {{
                    ""doctorName"": ""Dr. Name"",
                    ""specialist"": ""Specialist Type"",
                    ""medicines"": [
                        {{ 
                            ""drug"": ""Drug Name"", 
                            ""dose"": ""Dose"", 
                            ""freq"": ""Frequency"", 
                            ""notes"": ""Notes"",
                            ""confidence"": 95,
                            ""requiresManualReview"": false
                        }}
                    ]
                }}
            ";

            // 4. Prepare Payload for Groq (Llama 4)
            var requestBody = new
            {
                model = "meta-llama/llama-4-scout-17b-16e-instruct",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = prompt },
                            new
                            {
                                type = "image_url",
                                image_url = new
                                {
                                    url = $"data:image/jpeg;base64,{base64Image}"
                                }
                            }
                        }
                    }
                },
                temperature = 0.1, 
                max_completion_tokens = 1024
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_endpoint, jsonContent);
            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Groq Raw Response: {responseString}");

            if (!response.IsSuccessStatusCode)
            {
                return new Prescription { Medicines = new List<Medicine>(), Notes = "Error: " + response.StatusCode };
            }

            dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
            string fullText = jsonResponse.choices[0].message.content;

            string jsonBlock = ExtractJson(fullText);
            try 
            {
                // Try deserializing the new object format
                return JsonConvert.DeserializeObject<Prescription>(jsonBlock);
            }
            catch
            {
                // Fallback or Empty
                return new Prescription();
            }
        }



        private string ExtractJson(string text)
        {
            var match = Regex.Match(text, @"```json(.*?)```", RegexOptions.Singleline);
            if (match.Success) return match.Groups[1].Value.Trim();
            var arrayMatch = Regex.Match(text, @"\[(.*)\]", RegexOptions.Singleline);
            if (arrayMatch.Success) return arrayMatch.Value.Trim();
            return "{}";
        }
    }
}
