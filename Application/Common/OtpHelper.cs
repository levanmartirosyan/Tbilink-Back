using System.Security.Cryptography;

namespace Tbilink_Back.Services.Helpers
{
    public class OtpHelper
    {
        public static string GenerateCode(int length = 6)
        {
            var bytes = RandomNumberGenerator.GetBytes(4);
            int number = BitConverter.ToInt32(bytes, 0) & int.MaxValue;
            return (number % (int)Math.Pow(10, length)).ToString($"D{length}");
        }

        public static string HashCode(string code)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(code));
            return Convert.ToBase64String(hash);
        }
    }
}
