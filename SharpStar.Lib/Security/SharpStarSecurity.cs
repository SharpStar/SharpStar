using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SharpStar.Lib.Security
{
    //Credit to StarNet (https://github.com/SirCmpwn/StarNet/)
    public static class SharpStarSecurity
    {

        public static string GenerateHash(string account, string password, string challenge, int rounds)
        {

            var salt = Encoding.UTF8.GetBytes(account + challenge);
            var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            while (rounds > 0)
            {
                sha256.Initialize();
                sha256.TransformBlock(hash, 0, hash.Length, null, 0);
                sha256.TransformFinalBlock(salt, 0, salt.Length);
                hash = sha256.Hash;
                rounds--;
            }

            sha256.Dispose();

            return Convert.ToBase64String(hash);
        }

        public static string GenerateSalt()
        {

            var random = RandomNumberGenerator.Create();
            var rawSalt = new byte[24];

            random.GetBytes(rawSalt);

            var salt = Convert.ToBase64String(rawSalt);

            random.Dispose();

            return salt;

        }

    }
}
