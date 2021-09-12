using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;

namespace TodoApi.Utils
{
    public static class HashFunction
    {
        public static string[] CreateHashAndSalt(string password)
        {
            byte[] salt = new byte[128/8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256/8));

            string[] results = {Convert.ToBase64String(salt), hashed};
            return results;
        }
        public static bool CheckPassword(string password, string salt, string hash)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256/8));

            if (hash != hashed) return false;

            return true;
        }
    }
}