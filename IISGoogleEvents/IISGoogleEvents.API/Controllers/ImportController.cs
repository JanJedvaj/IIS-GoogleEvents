using IISGoogleEvents.API.Authorization;
using IISGoogleEvents.API.Dtos;
using IISGoogleEvents.Application.Dtos.Import;
using IISGoogleEvents.Application.Models;
using IISGoogleEvents.Application.Services;
using IISGoogleEvents.Infrastructure.Enums;
using Microsoft.AspNetCore.Mvc;

namespace IISGoogleEvents.API.Controllers;

[AuthorizeRoles(Roles.Admin)]
[Produces("application/json")]
[Consumes("multipart/form-data")]
public class ImportController(EventImportService importService) : BaseController
{
    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [ProducesResponseType(typeof(StandardResponse<ImportResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(StandardResponse<ImportResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Import([FromForm] ImportRequestDto request, CancellationToken cancellationToken)
    {
        if (request.XmlFile is null || request.XmlFile.Length == 0)
            return HandleResponse(StandardResponse<ImportResultDto>.Create(
                ResultStatus.BadRequest, message: "XML datoteka nije poslana."));

        if (request.JsonFile is null || request.JsonFile.Length == 0)
            return HandleResponse(StandardResponse<ImportResultDto>.Create(
                ResultStatus.BadRequest, message: "JSON datoteka nije poslana."));

        await using var xmlStream = request.XmlFile.OpenReadStream();
        await using var jsonStream = request.JsonFile.OpenReadStream();

        var result = await importService.ImportAsync(xmlStream, jsonStream, cancellationToken);

        var hasErrors = result.XmlErrors.Count > 0 || result.JsonErrors.Count > 0 || result.BusinessErrors.Count > 0;

        if (hasErrors)
        {
            var flattenedErrors = result.XmlErrors.Select(e => $"XML: {e}")
                .Concat(result.JsonErrors.Select(e => $"JSON: {e}"))
                .Concat(result.BusinessErrors.Select(e => $"Pravila: {e}"));

            return HandleResponse(StandardResponse<ImportResultDto>.Create(
                ResultStatus.BadRequest,
                result,
                "Uvoz nije uspio.",
                flattenedErrors));
        }

        return HandleResponse(StandardResponse<ImportResultDto>.Create(
            ResultStatus.Created,
            result,
            "Uvoz je uspio."));
    }
}
