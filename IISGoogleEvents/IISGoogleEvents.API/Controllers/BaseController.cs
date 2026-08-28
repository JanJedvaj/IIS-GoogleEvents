using IISGoogleEvents.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace IISGoogleEvents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected ActionResult HandleResponse<T>(StandardResponse<T> response) => response.Status switch
    {
        ResultStatus.Ok => Ok(response),
        ResultStatus.Created => StatusCode(StatusCodes.Status201Created, response),
        ResultStatus.BadRequest => BadRequest(response),
        ResultStatus.NotFound => NotFound(response),
        ResultStatus.Unauthorized => Unauthorized(response),
        ResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, response),
        ResultStatus.Conflict => Conflict(response),
        _ => StatusCode(StatusCodes.Status500InternalServerError, response)
    };
}
