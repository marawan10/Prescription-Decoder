using PrescriptionDecoder.Domain.Entities;

namespace PrescriptionDecoder.Application.Interfaces
{
    public interface IGeminiService
    {
         Task<Prescription> AnalyzePrescriptionImageAsync(Stream imageStream);
    }
}
