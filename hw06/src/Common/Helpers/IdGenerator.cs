using System;

namespace Common.Helpers
{
    public static class IdGenerator
    {
        public static string Generate()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
