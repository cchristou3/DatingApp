using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class ClaimPrincipleExtensions
    {
        public static string GetUsername(this ClaimsPrincipal user)
        {
            //* `ClaimTypes.Name` represents the `JwtRegisteredClaimNames.UniqueName` 
            //* that we have set up in the token service
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static int GetId(this ClaimsPrincipal user)
        {
            //* `ClaimTypes.NameIdentifier` represents the `JwtRegisteredClaimNames.NameId` 
            //* that we have set up inthe token service
            return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}