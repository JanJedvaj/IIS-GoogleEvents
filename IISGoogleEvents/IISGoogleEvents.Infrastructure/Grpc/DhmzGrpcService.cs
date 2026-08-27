using Grpc.Core;
using IISGoogleEvents.Application.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IISGoogleEvents.Infrastructure.Grpc;

public class DhmzGrpcService(
    DhmzHttpClient dhmzHttpClient,
    IOptions<DhmzConfig> config,
    ILogger<DhmzGrpcService> logger)
    : WeatherService.WeatherServiceBase
{
    public override async Task<WeatherResponse> GetWeatherByCity(WeatherRequest request, ServerCallContext context)
    {
        try
        {
            return await GetWeatherByCityStandardAsync(request.CityName, context.CancellationToken);
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            throw new RpcException(new Status(StatusCode.Cancelled, "Request was cancelled by the caller."));
        }
        catch (OperationCanceledException)
        {
            throw new RpcException(new Status(StatusCode.DeadlineExceeded, "DHMZ feed did not respond in time."));
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to reach the DHMZ feed.");
            throw new RpcException(new Status(StatusCode.Internal, "Could not reach the DHMZ weather feed."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure resolving weather for '{CityName}'.", request.CityName);
            throw new RpcException(new Status(StatusCode.Internal, "Unexpected error resolving weather."));
        }
    }

    private async Task<WeatherResponse> GetWeatherByCityStandardAsync(string cityName, CancellationToken callerToken)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(config.Value.TimeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(callerToken, timeoutCts.Token);

        var feed = await dhmzHttpClient.FetchWeatherDataAsync(linkedCts.Token);

        var matches = feed.Gradovi
            .Where(g => g.GradIme.Contains(cityName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 0)
            throw new KeyNotFoundException($"No city matching '{cityName}' was found in the DHMZ feed.");

        var response = new WeatherResponse
        {
            LastUpdated = $"{feed.DatumTermin.Datum.Trim()} {feed.DatumTermin.Termin.Trim()}:00"
        };

        response.Results.AddRange(matches.Select(g => new CityWeather
        {
            CityName = g.GradIme.Trim(),
            Temperature = g.Podatci.Temp.Trim(),
            Humidity = g.Podatci.Vlaga.Trim(),
            Pressure = g.Podatci.Tlak.Trim(),
            WindDirection = g.Podatci.VjetarSmjer.Trim(),
            WindSpeed = g.Podatci.VjetarBrzina.Trim(),
            Condition = g.Podatci.Vrijeme.Trim()
        }));

        return response;
    }
}
