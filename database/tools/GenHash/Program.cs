
using System.Security.Cryptography;
using System.Text;

Console.WriteLine(Hash("Admin@123"));
Console.WriteLine(Hash("Operator@123"));

static string Hash(string password)
{
    var salt = RandomNumberGenerator.GetBytes(16);
    var hash = Rfc2898DeriveBytes.Pbkdf2(
        Encoding.UTF8.GetBytes(password),
        salt,
        100_000,
        HashAlgorithmName.SHA256,
        32);
    return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
}
