using PrescriptionDecoder.Domain.Entities;

namespace PrescriptionDecoder.Application.Interfaces
{
    public interface IGroqService
    {
        Task<Prescription> AnalyzePrescriptionImageAsync(Stream imageStream);
    }
}
