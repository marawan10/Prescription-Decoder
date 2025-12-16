namespace PrescriptionDecoder.Application.Interfaces
{
    public interface IImagePreprocessingService
    {
        Stream PreprocessForOcr(Stream input);
        Stream PreprocessForAi(Stream input);
    }
}
