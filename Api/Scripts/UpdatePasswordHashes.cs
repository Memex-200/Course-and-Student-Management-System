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
            var preservedCount = 0;

            // Known admin credentials for password preservation
            var adminCredentials = new Dictionary<string, string>
            {
                { "admin", "123456" },
                { "ibrahem", "Ibrahem@123!" },
                { "karem", "Karem@123!" },
                { "ahmed", "Ahmed@123!" },
                { "mira", "Mira@123!" },
                { "memex", "Eman@123!" }
            };

            foreach (var user in users)
            {
                // Check if the password hash is not a BCrypt hash (BCrypt hashes start with $2a$, $2b$, or $2y$)
                if (!user.PasswordHash.StartsWith("$2"))
                {
                    // Try to preserve the original password if we know it
                    if (adminCredentials.TryGetValue(user.Username, out var knownPassword))
                    {
                        // Re-hash the known password using BCrypt
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(knownPassword);
                        preservedCount++;
                        Console.WriteLine($"Preserved and rehashed password for admin user: {user.Username} (ID: {user.Id})");
                    }
                    else
                    {
                        // For unknown users, set a default password
                        var defaultPassword = "TempPassword123!";
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
                        updatedCount++;
                        Console.WriteLine($"Reset password for user: {user.Username} (ID: {user.Id}) to default");
                    }
                }
            }

            if (updatedCount > 0 || preservedCount > 0)
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"Password migration completed:");
                Console.WriteLine($"- Preserved {preservedCount} admin passwords with original credentials");
                Console.WriteLine($"- Reset {updatedCount} user passwords to default");
                Console.WriteLine("All passwords now use BCrypt hashing.");
            }
            else
            {
                Console.WriteLine("All passwords are already using BCrypt. No updates needed.");
            }
        }
    }
}
