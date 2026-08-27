using Grpc.Core;
using IISGoogleEvents.API.Abstractions.Attributes;
using IISGoogleEvents.API.Abstractions.Controllers;
using IISGoogleEvents.Application.Models;
using IISGoogleEvents.Domain.Enums;
using IISGoogleEvents.Infrastructure.Grpc;
using Microsoft.AspNetCore.Mvc;

namespace IISGoogleEvents.API.Controllers;

[AuthorizeRoles(MinRole = Roles.User)]
public class WeatherController(WeatherService.WeatherServiceClient client) : BaseController
{
    [HttpGet]
    public async Task<ActionResult> GetByCity([FromQuery] string cityName, CancellationToken cancellationToken)
    {
        try
        {
            var reply = await client.GetWeatherByCityAsync(
                new WeatherRequest { CityName = cityName },
                cancellationToken: cancellationToken);

            return HandleResponse(StandardResponse<WeatherResponse>.Create(ResultStatus.Ok, reply));
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return HandleResponse(StandardResponse<WeatherResponse>.Create(
                ResultStatus.NotFound,
                message: ex.Status.Detail));
        }
        catch (RpcException ex)
        {
            return HandleResponse(StandardResponse<WeatherResponse>.Create(
                ResultStatus.InternalError,
                message: ex.Status.Detail));
        }
    }
}
