using Grpc.Core;
using IISGoogleEvents.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace IISGoogleEvents.API.Grpc;

[Authorize]
public class WeatherGrpcService(
    DhmzWeatherService weatherService,
    ILogger<WeatherGrpcService> logger)
    : WeatherService.WeatherServiceBase
{
    public override async Task<WeatherResponse> GetWeatherByCity(WeatherRequest request, ServerCallContext context)
    {
        try
        {
            var readings = await weatherService.GetByCityAsync(request.CityName, context.CancellationToken);

            var response = new WeatherResponse { LastUpdated = readings.LastUpdated };

            response.Results.AddRange(readings.Results.Select(r => new CityWeather
            {
                CityName = r.CityName,
                Temperature = r.Temperature,
                Humidity = r.Humidity,
                Pressure = r.Pressure,
                WindDirection = r.WindDirection,
                WindSpeed = r.WindSpeed,
                Condition = r.Condition
            }));

            return response;
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            throw new RpcException(new Status(StatusCode.Cancelled, "Poziv je prekinuo klijent."));
        }
        catch (OperationCanceledException)
        {
            throw new RpcException(new Status(StatusCode.DeadlineExceeded, "DHMZ feed nije odgovorio na vrijeme."));
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Nije moguće doći do DHMZ feeda.");
            throw new RpcException(new Status(StatusCode.Internal, "Nije moguće doći do DHMZ feeda."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Neočekivana greška pri dohvaćanju vremena za '{CityName}'.", request.CityName);
            throw new RpcException(new Status(StatusCode.Internal, "Neočekivana greška pri dohvaćanju vremena."));
        }
    }
}
