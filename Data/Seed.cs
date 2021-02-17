using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(DataContext context)
        {
            if(await context.Users.AnyAsync()) return;
            bool IsPrivileged = false;
            var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);
            foreach (var user in users)
            {
                using var hmac = new HMACSHA512();
                user.UserName = user.UserName.ToLower();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("passW0rd"));
                user.PasswordSalt = hmac.Key;
                if(!IsPrivileged)
                {
                    user.IsPrivileged = true;
                    IsPrivileged = true;
                }
                context.Users.Add(user);
            }
            await context.SaveChangesAsync();
        }

        public static async Task SeedDiscounts(DataContext context)
        {
            if(await context.Discounts.AnyAsync(o => o.IsPrivileged)) return;

            var discount = new Discount
            {
                DiscountPercent = 2,
                IsPrivileged = true
            };
            context.Discounts.Add(discount);            
            await context.SaveChangesAsync();
        }
    }
}