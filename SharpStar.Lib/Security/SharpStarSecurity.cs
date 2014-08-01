// SharpStar
// Copyright (C) 2014 Mitchell Kutchuk
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SharpStar.Lib.Security
{
    //Credit to StarNet (https://github.com/SirCmpwn/StarNet/)
    /// <summary>
    /// Contains various Starbound security helper methods
    /// </summary>
    public static class SharpStarSecurity
    {

        /// <summary>
        /// Generates a hash for use with Starbound
        /// </summary>
        /// <param name="account">The account name</param>
        /// <param name="password">The password</param>
        /// <param name="challenge">The challenge</param>
        /// <param name="rounds">The amount of rounds</param>
        /// <returns>The final hash</returns>
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
