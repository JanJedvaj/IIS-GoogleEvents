using IISGoogleEvents.Application.Dtos.Weather;
using IISGoogleEvents.Infrastructure.Clients.Dhmz;
using IISGoogleEvents.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace IISGoogleEvents.Application.Services;

public class DhmzWeatherService(DhmzClient dhmzClient, IOptions<DhmzOptions> config)
{
    public async Task<WeatherReadingsDto> GetByCityAsync(string cityName, CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(config.Value.TimeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        var feed = await dhmzClient.FetchWeatherDataAsync(linkedCts.Token);

        var matches = feed.Gradovi
            .Where(g => g.GradIme.Contains(cityName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(g => g.GradIme.Trim(), StringComparer.CurrentCulture)
            .ToList();

        if (matches.Count == 0)
            throw new KeyNotFoundException($"Nijedan grad koji odgovara '{cityName}' nije nađen u DHMZ feedu.");

        return new WeatherReadingsDto
        {
            LastUpdated = $"{feed.DatumTermin.Datum.Trim()} {feed.DatumTermin.Termin.Trim()}:00",
            Results = matches.Select(g => new CityWeatherDto
            {
                CityName = g.GradIme.Trim(),
                Temperature = g.Podatci.Temp.Trim(),
                Humidity = g.Podatci.Vlaga.Trim(),
                Pressure = g.Podatci.Tlak.Trim(),
                WindDirection = g.Podatci.VjetarSmjer.Trim(),
                WindSpeed = g.Podatci.VjetarBrzina.Trim(),
                Condition = g.Podatci.Vrijeme.Trim()
            }).ToList()
        };
    }
}
