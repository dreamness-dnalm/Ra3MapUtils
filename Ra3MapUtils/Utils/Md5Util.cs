using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Ra3MapUtils.Utils;

public static class Md5Util
{
    public static string CalculateFileMD5(string filePath)
    {
        using (FileStream fileStream = File.OpenRead(filePath))
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(fileStream);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // 转换为小写16进制
                }
                return sb.ToString().ToUpperInvariant();
            }
        }
    }
}