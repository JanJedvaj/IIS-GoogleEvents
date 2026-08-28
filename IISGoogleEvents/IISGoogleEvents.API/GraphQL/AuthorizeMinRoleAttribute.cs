using HotChocolate.Authorization;
using IISGoogleEvents.Infrastructure.Enums;

namespace IISGoogleEvents.API.GraphQL;

public sealed class AuthorizeMinRoleAttribute : AuthorizeAttribute
{
    public AuthorizeMinRoleAttribute(Roles minRole)
    {
        Roles = Enum.GetValues<Roles>()
            .Where(role => role >= minRole)
            .Select(role => role.ToString())
            .ToArray();
    }
}
