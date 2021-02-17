using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipalExtensions
    { 
        public static string GetUserName(this ClaimsPrincipal user)
        {
            string returnValue = string.Empty;
            if(user != null)
            {
               returnValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            return returnValue;
        }
    }
}