using LTC2.Shared.Models.Domain;
using LTC2.Shared.Models.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace LTC2.Webapps.MainApp.Utils
{
    public class TokenUtils
    {
        public static string TokenName = "token";

        private readonly AuthorizationSettings _autorizationSettings;

        public TokenUtils(AuthorizationSettings autorizationSettings)
        {
            _autorizationSettings = autorizationSettings;
        }

        public string GenerateToken(string user, string email, string name)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_autorizationSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Email, email),

                new Claim("StravaAthleteId", user),
                new Claim("StavaAthleteName", name),
                new Claim("RegisteredEmail", email)

            };

            var token = new JwtSecurityToken(_autorizationSettings.Issuer,
                _autorizationSettings.Audience,
                claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: credentials);


            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public AuthenticationHeaderValue GetAuthenticationHeader(HttpRequest request)
        {
            var authorizationHeader = request.Headers[HeaderNames.Authorization];

            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                var authHeader = AuthenticationHeaderValue.Parse(request.Headers[HeaderNames.Authorization]);
                return authHeader;
            }

            return null;
        }

        public bool ValidateToken(string token)
        {
            var securityTokenHandler = new JwtSecurityTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters()
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_autorizationSettings.Key)),
                ValidAudience = _autorizationSettings.ValidAudience,
                ValidIssuer = _autorizationSettings.ValidIssuers,
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true
            };

            try
            {
                SecurityToken validatedToken = null;

                securityTokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Profile GetProfileFormToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = GetToken(tokenHandler, token);

            var profile = new Profile()
            {
                AthleteId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                Name = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                Email = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
            };

            return profile;
        }

        public DateTime TokenIsValidUntil(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = GetToken(tokenHandler, token);


            return jwtToken.ValidTo;
        }

        private JwtSecurityToken GetToken(JwtSecurityTokenHandler tokenHandler, string encodedTokenValue)
        {
            if (string.IsNullOrEmpty(encodedTokenValue) || !tokenHandler.CanReadToken(encodedTokenValue))
            {
                return null;
            }

            return tokenHandler.ReadToken(encodedTokenValue) as JwtSecurityToken;
        }

    }
}
