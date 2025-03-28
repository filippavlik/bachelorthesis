using LoginPart.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.IO;

namespace LoginPart.Identity
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly string _secretKey;
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = _configuration["SecretKey"];
        }
	

        public string GenerateToken(Users user, string role)
        {
	    // Read the secrets from the Docker secrets directory
            var jwtIssuer = System.IO.File.ReadAllText("/run/secrets/JwtIssuer").Trim();
            var jwtAudience = System.IO.File.ReadAllText("/run/secrets/JwtAudience").Trim();

	    var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, role),
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
			expires: DateTime.Now.AddMinutes(30), // set time for expiration
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        
	}
	  private string GetDockerSecret(string secretName)
        {
                string path = $"/run/secrets/{secretName}";
                return System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path).Trim() : null;
        }
    }
}
