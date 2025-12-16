using PrescriptionDecoder.Application.Interfaces;
using PrescriptionDecoder.Domain.Entities;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Configuration;

namespace PrescriptionDecoder.Infrastructure.Services
{
    public class GeminiService : IGeminiService
    {
        // 1. Google API Key
        private readonly string _apiKey;

        // 2. Updated Endpoint -> gemini-1.5-flash (Best balance of Speed, Accuracy, and Free Quota)
        private readonly string _endpoint =
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
        private readonly IImagePreprocessingService _preprocessingService;

        public GeminiService(IImagePreprocessingService preprocessingService, IConfiguration configuration)
        {
            _preprocessingService = preprocessingService;
            _apiKey = configuration["ApiKeys:Gemini"];
        }

        public async Task<Prescription> AnalyzePrescriptionImageAsync(Stream imageStream)
        {
            using var client = new HttpClient();

            // 1. Convert Image (Use Preprocessed Image - Mild for AI)
            using var processedStream = _preprocessingService.PreprocessForAi(imageStream);
            using var memoryStream = new MemoryStream();
            await processedStream.CopyToAsync(memoryStream);
            byte[] imageBytes = memoryStream.ToArray();
            string base64Image = Convert.ToBase64String(imageBytes);

            // 2. Chain-of-Thought Prompt
            var prompt = @"
                Act as an Expert Clinical Pharmacist with 20 years of experience.
                Your task is to decode this handwritten prescription with 100% accuracy, specifically focusing on handling messy or overlapping handwriting.

                *** VISUAL DECONSTRUCTION & ANALYSIS ***
                1.  **Doctor & Specialist**: IDENTIFY the Doctor/Specialist FIRST. (e.g., Cardiologist, Dermatologist).
                2.  **Overlap handling**: If lines overlap, trace the letter strokes to separate words.
                3.  **Visual Evidence**: List the visible letters for each drug.
                4.  **Hypothesis Generation**: 
                    - Generate 3 candidates for each drug based on Vague Letters + Specialist Context.
                    - Example: ""Letters 'Aug', Specialist 'Dentist' -> Candidate 'Augmentin'"".

                *** KNOWLEDGE BASE: COMMON EGYPTIAN DRUGS ***
                - Antibiotics: Augmentin, Hibiotic, Curam, Megamox, Flumox, Klacid, Zithromax, Ciprofar.
                - Pain/NSAIDs: Panadol, Abimol, Cetal, Cataflam, Voltaren, Brufen, Ketolgin.
                - GI/Stomach: Antinal, Streptoquin, Visceralgine, Nexium, Controloc, Downoprazol.

                *** OUTPUT FORMAT ***
                First, provide a 'Chain of Thought' analyzing the specialist and visual evidence.
                Then, output the final result in a JSON block with this EXACT schema:
                {
                    ""doctorName"": ""Dr. Name"",
                    ""specialist"": ""Specialist Type"",
                    ""medicines"": [
                        { 
                            ""drug"": ""Drug Name"", 
                            ""dose"": ""Dose"", 
                            ""freq"": ""Frequency"", 
                            ""notes"": ""Notes"",
                            ""confidence"": 95,
                            ""requiresManualReview"": false
                        }
                    ]
                }

                Example Output:
                Thoughts: I see 'Dr. Magdy' and 'Internal Medicine'. The drug is 'Antinal'.
                ```json
                {
                    ""doctorName"": ""Dr. Magdy"",
                    ""specialist"": ""Internal Medicine"",
                    ""medicines"": [
                        { 
                            ""drug"": ""Antinal"", 
                            ""dose"": ""200mg"", 
                            ""freq"": ""1x3"", 
                            ""notes"": ""Standard GI drug"",
                            ""confidence"": 98,
                            ""requiresManualReview"": false
                        }
                    ]
                }
                ```

                Begin!
            ";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = prompt },
                            new { inline_data = new { mime_type = "image/jpeg", data = base64Image } }
                        }
                    }
                },
                safetySettings = new[]
                {
                    new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" }
                }
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_endpoint}?key={_apiKey}", jsonContent);
            var responseString = await response.Content.ReadAsStringAsync();

            // LOGGING FOR DEBUGGING
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"Gemini Response Status: {response.StatusCode}");
            Console.WriteLine($"Gemini Raw Response: {responseString}");
            Console.WriteLine("--------------------------------------------------");

            if (!response.IsSuccessStatusCode)
            {
                return new Prescription { Medicines = new List<Medicine>(), Notes = "Error: " + response.StatusCode };
            }

            dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);

            if (jsonResponse.candidates == null)
            {
                return new Prescription { Medicines = new List<Medicine>(), Notes = "Blocked by Safety Filters" };
            }

            string fullText = jsonResponse.candidates[0].content.parts[0].text;

            // 3. Extract JSON from the mixed response (Thoughts + JSON)
            string jsonBlock = ExtractJson(fullText);
            try
            {
                 return JsonConvert.DeserializeObject<Prescription>(jsonBlock);
            }
            catch
            {
                return new Prescription();
            }
        }

        private string ExtractJson(string text)
        {
            // Try to find content between ```json and ```
            var match = Regex.Match(text, @"```json(.*?)```", RegexOptions.Singleline);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            // Fallback: Try identifying the array brackets []
            var arrayMatch = Regex.Match(text, @"\[(.*)\]", RegexOptions.Singleline);
            if (arrayMatch.Success)
            {
                return arrayMatch.Value.Trim();
            }

            // If purely text was returned, return empty array to prevent crash
            return "{}";
        }
    }
}