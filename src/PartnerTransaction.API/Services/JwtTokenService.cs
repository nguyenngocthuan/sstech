using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PartnerTransaction.API.Services
{
    public interface JwtTokenService
    {
        string GenerateToken(
        string subject,
        string scope);
    }

    public class JwtTokenServiceImp(IConfiguration configuration) : JwtTokenService
    {
        public string GenerateToken(
            string subject,
            string scope)
        {
            var secretKey =
                configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException(
                    "JWT secret key is not configured.");

            var issuer = configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT issuer is not configured.");

            var audience = configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "JWT audience is not configured.");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, subject),
            new Claim("scope", scope)
        };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
    
}
