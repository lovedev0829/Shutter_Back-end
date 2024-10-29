using Microsoft.IdentityModel.Tokens;
using ShutterManagement.DAL.Context;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShutterManagement.Core;

namespace ShutterManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly ShutterDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(ShutterDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<string> LoginAsync(TblUser user)
        {
           

            var existingUser = await _context.TblUsers
                .SingleOrDefaultAsync(u => u.Username == user.Username && u.Pwd == user.Pwd);

           

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(AppConstant.JWTSecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", existingUser.Id.ToString()),
                    new Claim("Username", existingUser.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(AppConstant.JwtExpiryTime)),
                Issuer = AppConstant.JwtIssuer,
                Audience = AppConstant.JwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
