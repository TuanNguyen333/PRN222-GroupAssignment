using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Security
{
    /// <summary>
    /// Service to handle password hashing and verification using BCrypt
    /// </summary>
    public class BCryptPasswordService : IPasswordService
    {
        private readonly int _defaultWorkFactor;

        /// <summary>
        /// Initializes a new instance of the PasswordService
        /// </summary>
        /// <param name="defaultWorkFactor">The default work factor to use for hashing (default: 12)</param>
        public BCryptPasswordService(int defaultWorkFactor = 12)
        {
            _defaultWorkFactor = defaultWorkFactor;
        }

        /// <summary>
        /// Hashes a password using BCrypt
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>Hashed password</returns>
        public string HashPassword(string password)
        {
            return HashPassword(password, _defaultWorkFactor);
        }

        /// <summary>
        /// Hashes a password using BCrypt with a specific work factor
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <param name="workFactor">Work factor determining the hashing complexity</param>
        /// <returns>Hashed password</returns>
        public string HashPassword(string password, int workFactor)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password), "Password cannot be null or empty");

            if (workFactor < 4 || workFactor > 31)
                throw new ArgumentOutOfRangeException(nameof(workFactor), "Work factor must be between 4 and 31");

            // Generate a salt and hash the password
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
        }

        /// <summary>
        /// Verifies a password against a stored hash
        /// </summary>
        /// <param name="password">The password to check</param>
        /// <param name="hashedPassword">The stored hash to verify against</param>
        /// <returns>True if password matches, false otherwise</returns>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password), "Password cannot be null or empty");

            if (string.IsNullOrEmpty(hashedPassword))
                throw new ArgumentNullException(nameof(hashedPassword), "Hashed password cannot be null or empty");

            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
