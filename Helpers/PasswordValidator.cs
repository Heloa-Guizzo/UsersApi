using System.Text.RegularExpressions;

namespace UsersAPI.Helpers
{
    public static class PasswordValidator
    {
        public static void Validate(string password)
        {
            if (password.Length < 8)
                throw new ArgumentException("Password must have at least 8 characters.");

            if (!Regex.IsMatch(password, @"[A-Za-z]"))
                throw new ArgumentException("Password must contain letters.");

            if (!Regex.IsMatch(password, @"\d"))
                throw new ArgumentException("Password must contain numbers.");

            if (!Regex.IsMatch(password, @"[^A-Za-z0-9]"))
                throw new ArgumentException("Password must contain special characters.");
        }
    }
}
