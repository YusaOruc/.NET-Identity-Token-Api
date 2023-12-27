using IdentityApi.Data;
using IdentityApi.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _config;

        public AuthService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _config = config;
        }

        public async Task<bool> Logout()
        {
            await _signInManager.SignOutAsync(); // Oturumu kapat
            return true; // Başarıyla çıkış yapıldı
        }

        public async Task<IdentityResult> RegisterUser(string username, string password)
        {
            // Kullanıcı adının (UserName) benzersiz olmasını sağlamak için kontrol
            if (await _userManager.FindByNameAsync(username) != null)
            {
                // Kullanıcı adı zaten kullanımda ise hata döndür
                return IdentityResult.Failed(new IdentityError { Description = "Username is already taken." });
            }

            var user = new IdentityUser { UserName = username, Email = username };

            // Kullanıcının belirtilen rolu ile kaydedilmesi
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    // Belirtilen rol henüz yoksa, rolü oluşturun
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Kullanıcıya belirtilen rol atanıyor
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return result;
        }

        public async Task<SignInResult> LoginUser(string username, string password)
        {
            var user = await _userManager.FindByEmailAsync(username);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, password, false, lockoutOnFailure: false);

                return result;
            }

            return SignInResult.Failed;
        }

        public string GenerateTokenString(string username, string password)
        {
            // Kullanıcının rolünü al
            var user = _userManager.FindByNameAsync(username).Result;

            if (user == null)
            {
                // Kullanıcı adı zaten kullanımda ise hata döndür
                return "Invalid login attempt." ;
            }

            var roles = _userManager.GetRolesAsync(user).Result;

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username)
                };

            // Kullanıcının sahip olduğu rolleri ekleyin
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value));

            var signingCred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var securityToken = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(60),
                    issuer: _config.GetSection("Jwt:Issuer").Value,
                    audience: _config.GetSection("Jwt:Audience").Value,
                    signingCredentials: signingCred
                    );
            var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return token;
        }
    }
}
