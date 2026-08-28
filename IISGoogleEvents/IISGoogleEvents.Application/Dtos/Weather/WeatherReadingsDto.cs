namespace IISGoogleEvents.Application.Dtos.Weather;

public class WeatherReadingsDto
{
    public List<CityWeatherDto> Results { get; set; } = [];

    public string LastUpdated { get; set; } = string.Empty;
}

public class CityWeatherDto
{
    public string CityName { get; set; } = string.Empty;
    public string Temperature { get; set; } = string.Empty;
    public string Humidity { get; set; } = string.Empty;
    public string Pressure { get; set; } = string.Empty;
    public string WindDirection { get; set; } = string.Empty;
    public string WindSpeed { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
}
