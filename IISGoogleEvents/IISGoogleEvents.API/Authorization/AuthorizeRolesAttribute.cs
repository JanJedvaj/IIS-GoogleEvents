using IISGoogleEvents.Infrastructure.Enums;
using Microsoft.AspNetCore.Authorization;

namespace IISGoogleEvents.API.Authorization;

public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params Roles[] explicitRoles)
    {
        if (explicitRoles.Length > 0)
            Roles = string.Join(",", explicitRoles.Select(r => r.ToString()));
    }

    private Roles _minRole;

    public Roles MinRole
    {
        get => _minRole;
        set
        {
            _minRole = value;

            var atOrAbove = Enum.GetValues<Roles>()
                .Where(r => (int)r >= (int)value)
                .Select(r => r.ToString());

            Roles = string.IsNullOrEmpty(Roles)
                ? string.Join(",", atOrAbove)
                : string.Join(",", Roles.Split(',').Concat(atOrAbove).Distinct());
        }
    }
}
