using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API_Security.Services
{
    public class AuthorizationService : ITokenService
    {
        private IConfiguration configuration;

        public AuthorizationService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }


        public string DevolverToken(string email)
        {
            string key = configuration.GetValue<string>("Jwt:Key");

            ClaimsIdentity claimsIdentity = new ClaimsIdentity();

            Claim claim = new Claim(ClaimTypes.Email, email);

            claimsIdentity.AddClaim(claim);

            SigningCredentials signingCredentials = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);

            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = claimsIdentity,
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = signingCredentials
            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            var token = handler.CreateToken(securityTokenDescriptor);

            return handler.WriteToken(token);
        }
    }
}
