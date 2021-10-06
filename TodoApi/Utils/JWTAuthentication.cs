using System;
using System.Text;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace TodoApi.Utils
{
    public static class JWTAuthentication
    {
        public static string GenerateJwtToken(string userid)
        {
            // var tokenKey = Encoding.ASCII.GetBytes(key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] { 
                    new Claim(ClaimTypes.Name, userid), 
                    new Claim(ClaimTypes.Role, "user")
                }),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddHours(3),
                IssuedAt = DateTime.UtcNow,
                Issuer = "chitsanupong",
                Audience = "public",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Program.SecurityKey)), 
                    SecurityAlgorithms.HmacSha256Signature),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static string ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("1234567812345678")),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = "chitsanupong",
                    ValidAudience = "public",
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userid = jwtToken.Claims.First(x => x.Type == "unique_name").Value;

                // return account id from JWT token if validation successful
                return userid;
            }
            catch
            {
                // return null if validation fails
                return null;
            }
        }
    }
}