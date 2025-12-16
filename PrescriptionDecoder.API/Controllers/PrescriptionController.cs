using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PrescriptionDecoder.Domain.Entities;
using PrescriptionDecoder.Application.Interfaces;

namespace PrescriptionDecoder.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionController : ControllerBase
    {
        private readonly IGroqService _groqService;
        private readonly IGeminiService _geminiService;
        private readonly IFuzzyMatchingService _fuzzyService;

        public PrescriptionController(IGroqService groqService, IGeminiService geminiService, IFuzzyMatchingService fuzzyService)
        {
            _groqService = groqService;
            _geminiService = geminiService;
            _fuzzyService = fuzzyService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPrescription(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            try
            {
                // 1. Prepare Streams for Parallel Processing
                using var stream1 = new MemoryStream();
                using var stream2 = new MemoryStream();
                await file.CopyToAsync(stream1);
                stream1.Position = 0;
                await stream1.CopyToAsync(stream2);
                stream2.Position = 0;
                stream1.Position = 0;

                // 2. RUN PARALLEL AI (Groq + Gemini)
                var groqTask = _groqService.AnalyzePrescriptionImageAsync(stream1);
                var geminiTask = _geminiService.AnalyzePrescriptionImageAsync(stream2);

                await Task.WhenAll(groqTask, geminiTask);

                Prescription groqResult = await groqTask;
                Prescription geminiResult = await geminiTask;

                // 3. ENSEMBLE LOGIC (Pick the best results)
                Prescription finalResult;
                
                // If Groq is highly confident, trust it (it uses Llama Vision, usually smarter)
                if (groqResult.Medicines.Any(m => m.Confidence > 85))
                {
                    finalResult = groqResult;
                    Console.WriteLine("Selected Groq Result (High Confidence)");
                }
                // If both found items, but Groq is unsure, combine/check
                else if (geminiResult.Medicines.Count > 0)
                {
                    finalResult = geminiResult; // Fallback to Gemini if Groq failed or is low confidence
                    Console.WriteLine("Selected Gemini Result (Fallback)");
                }
                else
                {
                    finalResult = groqResult; // Default to Groq
                }

                // 4. APPLY SPELL CHECKER (Fuzzy Match) - RE-ENABLED
                foreach(var med in finalResult.Medicines)
                {
                    var original = med.Drug;
                    med.Drug = _fuzzyService.CorrectDrugName(original);

                    if (med.Drug != original)
                    {
                        med.Notes += $" [Auto-Corrected from '{original}']";
                        med.Confidence = 100; 
                    }
                    
                    // Low Confidence Flagging (Logic Step)
                    if (med.Confidence < 70)
                    {
                        med.RequiresManualReview = true;
                        med.Notes += " [Low Confidence - Check Manually]";
                    }
                }
                
                // Cleanup: Removed duplicate loop that was here when fuzzy was commented out

                return Ok(new { message = "Success (Dual AI Engine)", data = finalResult });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Controller Error: {ex.Message}");
                if(ex.InnerException != null) Console.WriteLine($"Inner: {ex.InnerException.Message}");
                return StatusCode(500, new { error = "Processing Error", details = ex.Message });
            }
        }


    }
}