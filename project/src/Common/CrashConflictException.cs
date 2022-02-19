namespace Common;

public class CrashConflictException : CrashException
{
    public CrashConflictException(string message) : base(message)
    {
    }
}