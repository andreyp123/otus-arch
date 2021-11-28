using UserManager.Common.Model;

namespace UserManager.TokenManager
{
    public interface ITokenManager
    {
        string IssueToken(User user);
    }
}
