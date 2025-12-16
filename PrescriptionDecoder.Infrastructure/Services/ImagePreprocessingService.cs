using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using PrescriptionDecoder.Application.Interfaces;

namespace PrescriptionDecoder.Infrastructure.Services
{
    public class ImagePreprocessingService : IImagePreprocessingService
    {
        public Stream PreprocessForOcr(Stream input)
        {
            if (input.CanSeek) input.Position = 0;
            using var image = Image.Load(input);

            // Aggressive processing for Legacy OCR
            image.Mutate(x => x
                .Grayscale()
                .Contrast(1.5f)
                .BinaryThreshold(0.55f));

            var outputMs = new MemoryStream();
            image.Save(outputMs, new JpegEncoder());
            outputMs.Position = 0;
            return outputMs;
        }

        public Stream PreprocessForAi(Stream input)
        {
            if (input.CanSeek) input.Position = 0;
            using var image = Image.Load(input);

            // Light processing for Modern AI Vision
            // Increased contrast to 1.5 (was 1.2) to help AI read faint text better, 
            // still avoiding BinaryThreshold to preserve grayscale details.
            image.Mutate(x => x
                .Grayscale()
                .Contrast(1.5f)); 

            var outputMs = new MemoryStream();
            image.Save(outputMs, new JpegEncoder());
            outputMs.Position = 0;
            return outputMs;
        }
    }
}
