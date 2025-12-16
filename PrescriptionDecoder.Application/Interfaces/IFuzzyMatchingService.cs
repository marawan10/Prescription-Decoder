using PrescriptionDecoder.Domain.Entities;

namespace PrescriptionDecoder.Application.Interfaces
{
    public interface IFuzzyMatchingService
    {
        string CorrectDrugName(string input);
    }
}
