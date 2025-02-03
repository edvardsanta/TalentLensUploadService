using DocumentFormat.OpenXml.Spreadsheet;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using UploadFiles.Services.Interfaces;
using UploadFiles.Services.Services.Upload;
namespace UploadFiles.Configurations
{
    public static class ServicesConfiguration
    {
        private static ImmutableList<IFileHandler> ConfigureFileHandlers() => [
                new PDFUpload(),
                new XLSXUpload()
            ];

        public static void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 209715200;
            });
            ConfigureMassTransit(services, config);
            services.AddScoped(_ => ConfigureFileHandlers());
            services.AddScoped<UploadManager>();

            services.ConfigureHttpJsonOptions(options =>
            {
            });
        }

        public static void ConfigureAuthentication(IServiceCollection services, IConfiguration config)
        {
            var key = Encoding.ASCII.GetBytes(config["Jwt-Secret"]);
            var keyId = GenerateKeyId(key);
            var signingKey = new SymmetricSecurityKey(key) { KeyId = keyId };
            var debugHandler = new JwtSecurityTokenHandler();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = false, 
                    ValidateAudience = false, 
                    ValidateLifetime = true, 
                    ClockSkew = TimeSpan.Zero,
                    RequireSignedTokens = true,
                    TryAllIssuerSigningKeys = true,
                };
                options.TokenValidationParameters.IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                {
                    if (kid == keyId)
                    {
                        return new[] { signingKey };
                    }
                    return Array.Empty<SecurityKey>();
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = async context =>
                    {
                        var token = context.Token;
                        if (string.IsNullOrEmpty(token)) return;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var ex = context.Exception;
                        Console.WriteLine("=== Authentication Failed ===");
                        Console.WriteLine($"Exception type: {ex.GetType().Name}");
                        Console.WriteLine($"Message: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated successfully.");
                        var token = context.Principal.Claims;
                        if (token != null)
                        {
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        }

        private static string GenerateKeyId(byte[] secret)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha256.ComputeHash(secret);
                return BitConverter.ToString(hash, 0, 8).Replace("-", "").ToLower();
            }
        }

        private static void ConfigureMassTransit(IServiceCollection services, IConfiguration config)
        {
            var rabbitMqUri = config["RabbitMq:Uri"];
            var rabbitMqUser = config["RabbitMq:Username"];
            var rabbitMqPass = config["RabbitMq:Password"];

            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqUri, "/", h =>
                    {
                        h.Username(rabbitMqUser);
                        h.Password(rabbitMqPass);
                    });
                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }

}
