using System.Text.Json;
using API.Helpers;
using Microsoft.AspNetCore.Http;

namespace API.Extensions
{
    public static class HttpExtensions
    {
        public static void AddingPagingHeader(this HttpResponse response,int currentPage, 
            int totalPages, int pageSize, int totalCount)
        {
            var paginationHeader = new PaginationHeader(currentPage,totalPages,pageSize,totalCount);
            var options = new JsonSerializerOptions
            {
                 PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            response.Headers.Add("pagination",JsonSerializer.Serialize(paginationHeader,options));
            response.Headers.Add("Access-Control-Expose-Headers","pagination");
        }
    }
}