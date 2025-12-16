using PrescriptionDecoder.Application.Interfaces;
using PrescriptionDecoder.Infrastructure.Services;

namespace PrescriptionDecoder.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // 1. Register Services
            builder.Services.AddScoped<IGeminiService, GeminiService>(); 
            builder.Services.AddScoped<IOcrSpaceService, OcrSpaceService>();
            builder.Services.AddScoped<IGroqService, GroqService>();
            builder.Services.AddScoped<IImagePreprocessingService, ImagePreprocessingService>();
            
            // Manual registration to inject path
            builder.Services.AddSingleton<IFuzzyMatchingService>(provider => 
            {
                var env = provider.GetRequiredService<IWebHostEnvironment>();
                var path = Path.Combine(env.ContentRootPath, "Data", "EgyptianDrugs.json"); // Data folder should be in API root
                return new FuzzyMatchingService(path);
            });

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // ????? CORS ???? ??? ???? ??? React ???? ???? ?????? ?????
            app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

            app.UseAuthorization();

            app.MapControllers();
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}