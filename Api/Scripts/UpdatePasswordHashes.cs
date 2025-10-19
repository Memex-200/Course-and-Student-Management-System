using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using BCrypt.Net;

namespace Api.Scripts
{
    public static class UpdatePasswordHashes
    {
        public static async Task UpdateAllPasswordsToBCrypt(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            var users = await context.Users.ToListAsync();
            var updatedCount = 0;

            foreach (var user in users)
            {
                // Check if the password hash is not a BCrypt hash (BCrypt hashes start with $2a$, $2b$, or $2y$)
                if (!user.PasswordHash.StartsWith("$2"))
                {
                    // This is likely an old SHA256 hash, we need to reset the password
                    // For security, we'll set a default password that users can change
                    var defaultPassword = "TempPassword123!";
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
                    updatedCount++;
                    
                    Console.WriteLine($"Updated password for user: {user.Username} (ID: {user.Id})");
                }
            }

            if (updatedCount > 0)
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"Updated {updatedCount} user passwords to use BCrypt.");
                Console.WriteLine("All users now have the temporary password: TempPassword123!");
                Console.WriteLine("Users should change their passwords after first login.");
            }
            else
            {
                Console.WriteLine("All passwords are already using BCrypt. No updates needed.");
            }
        }
    }
}
