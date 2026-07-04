using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Example.Common.Models;
using Example.Common.Const;
using Example.Common.Utilities;
using System.IdentityModel.Tokens.Jwt;

namespace Example.Common.Base
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected Guid GetUserId()
        {
            Guid userId = Guid.Empty;
            if (!StaticVariable.AuthSetting.IsCheckToken)
            {
                userId = StaticVariable.AuthSetting.UserId;
            }
            else
            {
                var jwtToken = this.GetJwtSecurityToken();
                if (jwtToken != null)
                {
                    userId = jwtToken.Claims.First(claim => claim.Type.ToLower() == "sub").Value.ToGuid();
                }
            }
            return userId;
        }

        private JwtSecurityToken? GetJwtSecurityToken()
        {
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                return new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            }
            return null;
        }

    }
}
