using Microsoft.AspNetCore.Http;

namespace HRDashboard.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file); // Add this method
    }
}