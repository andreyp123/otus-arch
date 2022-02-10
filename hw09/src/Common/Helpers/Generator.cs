namespace Common.Helpers;

public static class Generator
{
    public static string GenerateId()
    {
        return Guid.NewGuid().ToString();
    }

    public static string GenerateApiKey()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}