using System.Security.Cryptography;

namespace Grow.Infrastructure.Auth;

public static class SessionKeyGenerator
{
    public static string Generate()
        => RandomNumberGenerator.GetHexString(48).ToUpper();
}