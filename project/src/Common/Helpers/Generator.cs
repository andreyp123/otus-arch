namespace Common.Helpers;

public static class Generator
{
    private const string ApiKeyAlphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int ApiKeyLen = 32;
    private static readonly Random Rand = Random.Shared;
    
    public static string GenerateId()
    {
        return Guid.NewGuid().ToString();
    }

    public static string GenerateApiKey()
    {
        return new string(Enumerable
            .Repeat(0, ApiKeyLen)
            .Select(_ => ApiKeyAlphabet[Rand.Next(ApiKeyAlphabet.Length)])
            .ToArray());
    }
}