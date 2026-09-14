using System.Security.Claims;

namespace NexaMart.Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var idClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var id) ? id : 0;
    }

    public static string GetUserEmail(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    }

    public static string GetUserFullName(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
    }

    public static string GetUserPhoneNumber(this ClaimsPrincipal user)
    {
        return user.FindFirstValue("PhoneNumber") ?? string.Empty;
    }
}
