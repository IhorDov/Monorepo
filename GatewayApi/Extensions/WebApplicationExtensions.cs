using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GatewayApi.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplicationBuilder AddAppAuthentication(this WebApplicationBuilder builder)
        {
            var settingSection = builder.Configuration.GetSection("JwtSettings");

            var token = settingSection.GetSection("Token").Value!;
            var issuer = settingSection.GetSection("Issuer").Value!;
            var audience = settingSection.GetSection("Audience").Value!;

            var key = Encoding.ASCII.GetBytes(token);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true
                    };
                });
            return builder;
        }
    }
}
