using IISGoogleEvents.API.Abstractions.Attributes;
using IISGoogleEvents.API.Abstractions.Controllers;
using IISGoogleEvents.Application.Constants;
using IISGoogleEvents.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace IISGoogleEvents.API.Controllers;

/// <summary>
/// Smoke-test surface for the role gate. Delete once the events controller
/// (requirement 5) provides real endpoints at each access level.
/// </summary>
public class PingController : BaseController
{
    [HttpGet("anonymous")]
    public ActionResult Anonymous() => Ok(new { message = "No token required." });

    [AuthorizeRoles(MinRole = Roles.User)]
    [HttpGet("user")]
    public ActionResult UserOnly() => Ok(new
    {
        message = "Reachable by User and Admin.",
        username = User.Identity?.Name,
        role = User.FindFirst(CustomClaimTypes.Role)?.Value
    });

    [AuthorizeRoles(MinRole = Roles.Admin)]
    [HttpGet("admin")]
    public ActionResult AdminOnly() => Ok(new { message = "Reachable by Admin only." });
}
