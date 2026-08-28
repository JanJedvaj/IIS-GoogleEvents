using IISGoogleEvents.API.Authorization;
using IISGoogleEvents.Application.Dtos.Weather;
using IISGoogleEvents.Application.Models;
using IISGoogleEvents.Application.Services;
using IISGoogleEvents.Infrastructure.Enums;
using Microsoft.AspNetCore.Mvc;

namespace IISGoogleEvents.API.Controllers;

[AuthorizeRoles(MinRole = Roles.User)]
[Produces("application/json")]
public class WeatherController(DhmzWeatherService weatherService) : BaseController
{
    [HttpGet]
    [ProducesResponseType(typeof(StandardResponse<WeatherReadingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(StandardResponse<WeatherReadingsDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(StandardResponse<WeatherReadingsDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetByCity([FromQuery] string? cityName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            return HandleResponse(StandardResponse<WeatherReadingsDto>.Create(
                ResultStatus.BadRequest, message: "Naziv grada je obavezan."));

        try
        {
            var readings = await weatherService.GetByCityAsync(cityName, cancellationToken);
            return HandleResponse(StandardResponse<WeatherReadingsDto>.Create(ResultStatus.Ok, readings));
        }
        catch (KeyNotFoundException ex)
        {
            return HandleResponse(StandardResponse<WeatherReadingsDto>.Create(ResultStatus.NotFound, message: ex.Message));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return HandleResponse(StandardResponse<WeatherReadingsDto>.Create(
                ResultStatus.InternalError, message: "DHMZ feed nije odgovorio na vrijeme."));
        }
        catch (HttpRequestException)
        {
            return HandleResponse(StandardResponse<WeatherReadingsDto>.Create(
                ResultStatus.InternalError, message: "Nije moguće doći do DHMZ feeda."));
        }
    }
}
