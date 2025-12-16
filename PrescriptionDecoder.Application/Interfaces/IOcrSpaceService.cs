namespace PrescriptionDecoder.Application.Interfaces
{
    public interface IOcrSpaceService
    {
        Task<string> GetRawTextAsync(Stream imageStream);
    }
}
