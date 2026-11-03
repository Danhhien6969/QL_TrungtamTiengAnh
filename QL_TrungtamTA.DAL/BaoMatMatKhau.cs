
using System;
using System.Security.Cryptography;

namespace QL_TrungtamTA.BLL
{
    public static class BaoMatMatKhau
    {
        private const int SoVongLap = 600000;

        // Băm mật khẩu trước khi lưu SQL
        public static string BamMatKhau(string matKhau)
        {
            byte[] salt =
                RandomNumberGenerator.GetBytes(16);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                matKhau,
                salt,
                SoVongLap,
                HashAlgorithmName.SHA256,
                32);

            return "PBKDF2-SHA256$" +
                SoVongLap + "$" +
                Convert.ToBase64String(salt) + "$" +
                Convert.ToBase64String(hash);
        }

        // Kiểm tra mật khẩu đăng nhập
        public static bool KiemTraMatKhau(
            string matKhau,
            string matKhauHash)
        {
            try
            {
                string[] parts = matKhauHash.Split('$');

                if (parts.Length != 4 ||
                    parts[0] != "PBKDF2-SHA256")
                    return false;

                if (!int.TryParse(
                    parts[1], out int soVongLap))
                    return false;

                if (soVongLap < 100000 ||
                    soVongLap > 1000000)
                    return false;

                byte[] salt =
                    Convert.FromBase64String(parts[2]);

                byte[] hashCu =
                    Convert.FromBase64String(parts[3]);

                if (salt.Length != 16 ||
                    hashCu.Length != 32)
                    return false;

                byte[] hashMoi =
                    Rfc2898DeriveBytes.Pbkdf2(
                        matKhau,
                        salt,
                        soVongLap,
                        HashAlgorithmName.SHA256,
                        hashCu.Length);

                return CryptographicOperations
                    .FixedTimeEquals(hashMoi, hashCu);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
