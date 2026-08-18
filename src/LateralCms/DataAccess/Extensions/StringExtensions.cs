using System.Security.Cryptography;
using System.Text;

namespace LateralCms.Domain.Extensions;

public static class StringExtensions
{
    public static string Encrypt(this string input)
    {
        var data = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(data);

        return Convert.ToHexString(hashBytes);
    }
}
