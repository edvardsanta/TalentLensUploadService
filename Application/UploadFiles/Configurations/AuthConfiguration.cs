using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace UploadFiles.Configurations
{
    public static class AuthConfiguration
    {
        public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, IConfiguration config)
        {
            string? jwtSecret = config.GetRequiredSection("JWT_SECRET").Value;

            ArgumentException.ThrowIfNullOrEmpty(jwtSecret);

            byte[] key = Encoding.ASCII.GetBytes(jwtSecret);
            string keyId = GenerateKeyId(key);
            var signingKey = new SymmetricSecurityKey(key) { KeyId = keyId };
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
                    OnMessageReceived = context =>
                    {
                        var token = context.Token;
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var ex = context.Exception;
                        Console.WriteLine("=== Authentication Failed ===");
                        Console.WriteLine($"Exception type: {ex.GetType().Name}");
                        Console.WriteLine($"Message: {ex.Message}");
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

            return services;
        }

        private static string GenerateKeyId(byte[] secret)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha256.ComputeHash(secret);
                return BitConverter.ToString(hash, 0, 8).Replace("-", "").ToLower();
            }
        }
    }
}
