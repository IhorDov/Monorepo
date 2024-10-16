using GameServerAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Security.Cryptography.X509Certificates;
using System.Text;

//1
//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<IGameServerService, GameServerService>();

//// Add CORS policies
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll",
//        builder =>
//        {
//            builder.AllowAnyOrigin()
//                   .AllowAnyMethod()
//                   .AllowAnyHeader();
//        });
//});

////builder.Services.AddSwaggerGen(options =>
////{
////    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
////    {
////        In = ParameterLocation.Header,
////        Name = "Authorization",
////        Type = SecuritySchemeType.ApiKey
////    });

////    options.OperationFilter<SecurityRequirementsOperationFilter>();
////});

//// Configure Swagger for JWT Authorization
//builder.Services.AddSwaggerGen(options =>
//{
//    options.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Version = "v1",
//        Title = "GameServer API",
//        Description = "Authentication with JWT",
//    });

//    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "Bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header
//    });

//    options.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            }, Array.Empty<string>()
//        }
//    });
//});

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
//                builder.Configuration.GetSection("AppSettings:Token").Value!)),
//            ValidIssuers = new[] { "http://loginapi" },
//            ValidAudiences = new[] { "http://loginapi" }
//        };
//        options.Events = new JwtBearerEvents
//        {
//            OnAuthenticationFailed = context =>
//            {
//                Console.WriteLine("Token failed validation: " + context.Exception.Message);
//                Console.WriteLine("Token som I got from LoginApi: " + context.Scheme);
//                return Task.CompletedTask;
//            }
//        };
//    });

////builder.Services.AddAuthentication().AddJwtBearer(options =>
////{
////    options.TokenValidationParameters = new TokenValidationParameters
////    {
////        ValidateIssuerSigningKey = true,
////        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
////                builder.Configuration.GetSection("AppSettings:Token").Value!)),
////        ValidateIssuer = false,
////        ValidateAudience = false
////    };
////    options.Events = new JwtBearerEvents
////    {
////        OnAuthenticationFailed = context =>
////        {
////            Console.WriteLine("Token failed validation: " + context.Exception.Message);
////            return Task.CompletedTask;
////        }
////    };
////});


//var app = builder.Build();

//app.UseSwagger();
//app.UseSwaggerUI();
////app.UseCors(policy =>
////{
////    policy.WithOrigins("http://localhost:7216")
////        .AllowAnyHeader()
////        .AllowAnyMethod()
////        .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization);
////});


//// Apply global CORS policy (be cautious of using AllowAll in production)
//app.UseCors("AllowAll");

//// Only enable HTTPS redirection in non-development environments.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseHttpsRedirection();
//}
////app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();
//app.Run();


//2
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IGameServerService, GameServerService>();

// Add CORS policies
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "GameServer API",
        Description = "Authentication with JWT",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration.GetSection("AppSettings:Token").Value!)),
            ValidIssuers = new[] { "http://loginapi" },
            ValidAudiences = new[] { "http://loginapi" }
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Token failed validation: " + context.Exception.Message);
                Console.WriteLine("Token som I got from LoginApi: " + context.Scheme.ToString());
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
