using IISGoogleEvents.API.Authorization;
using IISGoogleEvents.Application.Dtos.Soap;
using IISGoogleEvents.Application.Models;
using IISGoogleEvents.Application.Services;
using IISGoogleEvents.Infrastructure.Enums;
using Microsoft.AspNetCore.Mvc;

namespace IISGoogleEvents.API.Controllers;

[Route("api/events/xml")]
[Produces("application/json")]
public class EventXmlController(XmlExportService export) : BaseController
{
    [HttpPost("generate")]
    [AuthorizeRoles(Roles.Admin)]
    [ProducesResponseType(typeof(StandardResponse<XmlExportResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Generate(CancellationToken cancellationToken)
    {
        var result = await export.GenerateAsync(cancellationToken);

        var message = result.EventCount == 0
            ? "XML datoteka je generirana, ali ne sadrži nijedan događaj pa neće proći XSD validaciju."
            : "XML datoteka je generirana.";

        return HandleResponse(StandardResponse<XmlExportResultDto>.Create(ResultStatus.Ok, result, message));
    }

    [HttpGet]
    [AuthorizeRoles(MinRole = Roles.User)]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(StandardResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Download()
    {
        try
        {
            var stream = export.OpenRead();
            return File(stream, "application/xml", "events.xml");
        }
        catch (FileNotFoundException)
        {
            return HandleResponse(StandardResponse<object>.Create(
                ResultStatus.NotFound, message: "XML datoteka još nije generirana."));
        }
    }
}
