using Example.Common.Const;
using Example.Common.Entities;
using Example.Common.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Example.Common.Utilities.Helper
{
    public static class TokenHelper
    {
        public static string GenerateRandomKey(int length = 32)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                // Một khóa bí mật thông thường có độ dài từ 128 đến 256 bit (16-32 ký tự)
                byte[] keyData = new byte[length]; // Độ dài của token (số byte)
                rng.GetBytes(keyData);

                string randomKey = Convert.ToBase64String(keyData);
                return randomKey;
            }
        }

        public static string GenerateTokenFromData(string data)
        {
            string randomKey = GenerateRandomKey();
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            string keyData = data + timestamp;
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(randomKey)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(keyData));
                string token = Convert.ToBase64String(hashBytes);
                return token;
            }
        }

        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public static JwtSecurityToken CreateToken(List<Claim> authClaims, DateTime? expire = null)
        {
            expire ??= DateTime.Now.AddMinutes(StaticVariable.JWTConfig.TokenValidityInMinutes);

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(StaticVariable.JWTConfig.Secret));

            var token = new JwtSecurityToken(
                issuer: StaticVariable.JWTConfig.ValidIssuer,
                audience: StaticVariable.JWTConfig.ValidAudience,
                claims: authClaims,
                expires: expire,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        //public static List<Claim> InitClaim(ApplicationUser userInfo,
        //            string avatar,
        //            List<string> listGroupRole,
        //            List<string> listRoleClaim,
        //            IEnumerable<UserRoleDataJwtDTO> listArea,
        //            IEnumerable<UserRoleDataJwtDTO> listDepartment,
        //            string customerCode)
        //{
        //    List<Claim> lstClaim = new List<Claim>
        //    {
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //        new Claim(ClaimConst.UserId, userInfo.Id.ToString()),
        //        new Claim(ClaimConst.CustomerId, userInfo.CustomerId.ToString()),
        //        new Claim(ClaimConst.UserType, userInfo.UserType.ToString()),
        //        new Claim(ClaimTypes.Name, userInfo.UserName)
        //    };

        //    if (!string.IsNullOrEmpty(userInfo.FullName))
        //    {
        //        lstClaim.Add(new Claim(ClaimConst.FullName, userInfo.FullName));
        //    }

        //    if (!string.IsNullOrEmpty(userInfo.Email))
        //    {
        //        lstClaim.Add(new Claim(ClaimConst.Email, userInfo.Email));
        //    }

        //    //if (!string.IsNullOrEmpty(avatar))
        //    //{
        //    //    lstClaim.Add(new Claim(ClaimConst.Avatar, avatar));
        //    //}

        //    if (!string.IsNullOrEmpty(customerCode))
        //    {
        //        lstClaim.Add(new Claim(ClaimConst.CustomerCode, customerCode));
        //    }

        //    if (listGroupRole != null && listGroupRole.Any())
        //    {
        //        //lstClaim.Add(new Claim(ClaimConst.GroupRoles, string.Join(",", listGroupRole)));
        //        foreach (var group in listGroupRole)
        //        {
        //            lstClaim.Add(new Claim(ClaimConst.GroupRoles, group));
        //        }
        //    }

        //    if (listRoleClaim != null && listRoleClaim.Any())
        //    {
        //        foreach (var userRole in listRoleClaim)
        //        {
        //            lstClaim.Add(new Claim(ClaimTypes.Role, userRole));
        //        }
        //    }

        //    if (listArea != null && listArea.Any())
        //    {
        //        //foreach (var areaId in listArea)
        //        //{
        //        //    lstClaim.Add(new Claim(ClaimConst.Area, areaId.ToString()));
        //        //}

        //        string jsonData = JsonSerializer.Serialize(listArea);
        //        lstClaim.Add(new Claim(ClaimConst.Areas, jsonData));
        //    }

        //    if (listDepartment != null && listDepartment.Any())
        //    {
        //        //foreach (var deparmentId in listDepartment)
        //        //{
        //        //    lstClaim.Add(new Claim(ClaimConst.Department, deparmentId.ToString()));
        //        //}
        //        //lstClaim.Add(new Claim(ClaimConst.Department, string.Join(",", listDepartment)));

        //        string jsonData = JsonSerializer.Serialize(listDepartment);
        //        lstClaim.Add(new Claim(ClaimConst.Departments, jsonData));
        //    }

        //    lstClaim.Add(new Claim(JwtRegisteredClaimNames.Aud, StaticVariable.JWTConfig.ValidAudience));
        //    //lstClaim.Add(new Claim(JwtRegisteredClaimNames.Aud, _configuration["JWT:ValidAudience1"]));

        //    return lstClaim;
        //}

        public static ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(StaticVariable.JWTConfig.Secret)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;

        }

    }
}
