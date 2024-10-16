using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GameServerAPI.Services
{
    public class RegisterService
    {
        public Task<string> VerifyJWT(string jwt)
        {
            var secretPath = Environment.GetEnvironmentVariable("secretPath");
            if (string.IsNullOrEmpty(secretPath) || !System.IO.File.Exists(secretPath))
            {
                Console.WriteLine("Secret path is not set or file does not exist");
            }

            var secretValue = System.IO.File.ReadAllText(secretPath!);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretValue!));

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidIssuer = "http://loginapi",
                ValidAudience = "http://loginapi",
            };

            try
            {
                // Validate the token and return the principal (claims)
                var principal = tokenHandler.ValidateToken(jwt, validationParameters, out SecurityToken validatedToken);

                // You can access token properties like claims and expiration here
                var jwtToken = (JwtSecurityToken)validatedToken;
                return Task.FromResult("7777");
            }
            catch (SecurityTokenException ex)
            {
                // Token validation failed
                Console.WriteLine($"Token validation failed: {ex.Message}");
                return Task.FromResult("Failed");
            }
        }
    }
}
