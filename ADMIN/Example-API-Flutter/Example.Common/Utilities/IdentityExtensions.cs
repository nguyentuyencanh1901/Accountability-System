using System.Security.Claims;

namespace Example.Common.Utilities
{
    public static class IdentityExtension
    {
        public static string GetSpecificClaim(this ClaimsIdentity claimsIdentity, string claimType)
        {
            Claim? claim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == claimType);
            return claim != null ? claim.Value : string.Empty;
        }

        public static string GetRolesByClaim(this ClaimsIdentity claimsIdentity, string claimType)
        {
            IEnumerable<Claim> lstClaim = claimsIdentity.Claims.Where(x => x.Type == claimType);
            return lstClaim != null ? string.Join(",", lstClaim.Select(x => x.Value)) : string.Empty;
        }

        public static IEnumerable<Claim> GetListRolesByClaim(this ClaimsIdentity claimsIdentity, string claimType)
        {
            IEnumerable<Claim> lstClaim = claimsIdentity.Claims.Where(x => x.Type == claimType);
            return lstClaim;
        }

        public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            Claim? claim = ((ClaimsIdentity)claimsPrincipal.Identity).Claims.Single(x => x.Type == ClaimTypes.NameIdentifier);
            return Guid.Parse(claim.Value);
        }

        public static string GetSpecificClaim(this IEnumerable<Claim> lstClaim, string claimType)
        {
            Claim? claim = lstClaim.FirstOrDefault(x => x.Type == claimType);
            return claim != null ? claim.Value : string.Empty;
        }
    }
}
