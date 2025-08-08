using System.Security.Cryptography;
using System.Text;

namespace Api.Services
{
    public interface IPasswordGeneratorService
    {
        string GenerateRandomPassword(int length = 8);
        string GenerateUsername(string fullName);
    }

    public class PasswordGeneratorService : IPasswordGeneratorService
    {
        private const string LowerCaseLetters = "abcdefghijklmnopqrstuvwxyz";
        private const string UpperCaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Numbers = "0123456789";
        private const string SpecialCharacters = "!@#$%&*";

        public string GenerateRandomPassword(int length = 8)
        {
            if (length < 4)
                length = 8;

            var allCharacters = LowerCaseLetters + UpperCaseLetters + Numbers + SpecialCharacters;
            var password = new StringBuilder();

            using (var rng = RandomNumberGenerator.Create())
            {
                // Ensure at least one character from each category
                password.Append(GetRandomCharacter(LowerCaseLetters, rng));
                password.Append(GetRandomCharacter(UpperCaseLetters, rng));
                password.Append(GetRandomCharacter(Numbers, rng));
                password.Append(GetRandomCharacter(SpecialCharacters, rng));

                // Fill the rest with random characters
                for (int i = 4; i < length; i++)
                {
                    password.Append(GetRandomCharacter(allCharacters, rng));
                }
            }

            // Shuffle the password to avoid predictable patterns
            return ShuffleString(password.ToString());
        }

        public string GenerateUsername(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "student" + DateTime.Now.Ticks.ToString().Substring(0, 6);

            // Remove special characters and spaces, convert to lowercase
            var cleanName = new string(fullName.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToLower();
            
            // Take first 6 characters and add random numbers
            var baseName = cleanName.Length >= 6 ? cleanName.Substring(0, 6) : cleanName;
            var randomSuffix = new Random().Next(100, 999);
            
            return baseName + randomSuffix;
        }

        private char GetRandomCharacter(string characterSet, RandomNumberGenerator rng)
        {
            var randomBytes = new byte[1];
            rng.GetBytes(randomBytes);
            var randomIndex = randomBytes[0] % characterSet.Length;
            return characterSet[randomIndex];
        }

        private string ShuffleString(string input)
        {
            var array = input.ToCharArray();
            var random = new Random();
            
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
            
            return new string(array);
        }
    }
} 