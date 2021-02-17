using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Http;

namespace API.Interfaces 
{
    public interface IPhotoService
    {
        Task<Photo> AddPhotoAsync(HttpContext httpContext, IFormFile file);
        bool DeletePhotoAsync(string publicId);
    }
}