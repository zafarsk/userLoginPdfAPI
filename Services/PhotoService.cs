using System.IO;
using System.Threading.Tasks;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace API.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IConfiguration _config;
        public PhotoService(IConfiguration config)
        {
            _config = config;

        } 
        public async Task<Photo> AddPhotoAsync(HttpContext httpContext, IFormFile file)
        {
            string fileName = httpContext.User.GetUserName() + Path.GetRandomFileName() + ".png";
            string directory = _config["StoredFilesPath"];
            string url = string.Format("{0}://{1}/{2}/{3}", httpContext.Request.Scheme,
                                    httpContext.Request.Host.Value, directory, fileName);
            var filePath = Path.Combine(new string[]{ Directory.GetCurrentDirectory(),
                                            directory, fileName});

            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            return new Photo
            {
                Url = url,
                PublicId = fileName
            };
        }

        public bool DeletePhotoAsync(string publicId)
        {
            bool retValue = false;
            string directory = _config["StoredFilesPath"];
            var filePath = Path.Combine(new string[]{ Directory.GetCurrentDirectory(),
                                            directory, publicId});
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    retValue = true;
                }
                catch
                {
                    throw;
                }
            }

            return retValue;

        }
    }
}