using Microsoft.IdentityModel.Tokens;
using SharingModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Messenger.Services.SignIn
{
    public record UserInfo(string Id, string email ,string username);
    public  class JWTToken
    {
        public static string CreateToken(UserInfo user )
        {
            var audienceKey = Environment.GetEnvironmentVariable("Messenger:Audience");
            var issuerKey = Environment.GetEnvironmentVariable("Messenger:Issuer");
            List<Claim> claims = new List<Claim>
            {
               
                new Claim("sub" , user.Id),
                new Claim("aud",audienceKey),
                new Claim("iss",issuerKey),
            };
            
            var envKey = Environment.GetEnvironmentVariable("JWT:Messenger:Key");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(envKey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(50),
                    signingCredentials: cred
                    );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}
