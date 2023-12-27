using Microsoft.AspNetCore.Identity;

namespace IdentityApi.Interfaces
{
    public interface IAuthService
    {
        Task<bool> Logout();
        Task<IdentityResult> RegisterUser(string username, string password);
        Task<SignInResult> LoginUser(string username, string password);
        string GenerateTokenString(string username, string password);
    }
}
