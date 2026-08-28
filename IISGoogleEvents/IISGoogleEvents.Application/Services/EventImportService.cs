using System.Text.Json;
using System.Xml.Serialization;
using IISGoogleEvents.Application.Dtos.Import;
using IISGoogleEvents.Application.Mappers;
using IISGoogleEvents.Infrastructure.Repositories;

namespace IISGoogleEvents.Application.Services;

public class EventImportService(
    CalendarEventRepository repository,
    XmlValidationService xmlValidator,
    JsonValidationService jsonValidator)
{
    public async Task<ImportResultDto> ImportAsync(Stream xmlStream, Stream jsonStream, CancellationToken cancellationToken = default)
    {
        var result = new ImportResultDto();

        var xmlBuffer = await BufferAsync(xmlStream, cancellationToken);
        var jsonBuffer = await BufferAsync(jsonStream, cancellationToken);

        var xmlValidation = xmlValidator.Validate(new MemoryStream(xmlBuffer));
        var jsonValidation = jsonValidator.Validate(new MemoryStream(jsonBuffer));

        result.XmlErrors.AddRange(xmlValidation.Errors);
        result.JsonErrors.AddRange(jsonValidation.Errors);

        if (!xmlValidation.IsValid || !jsonValidation.IsValid)
            return result;

        List<ImportCalendarEventDto> fromXml;
        List<ImportCalendarEventDto> fromJson;

        try
        {
            fromXml = DeserializeXml(xmlBuffer);
        }
        catch (Exception ex)
        {
            result.XmlErrors.Add($"Deserijalizacija XML-a nije uspjela: {ex.Message}");
            return result;
        }

        try
        {
            fromJson = DeserializeJson(jsonBuffer);
        }
        catch (Exception ex)
        {
            result.JsonErrors.Add($"Deserijalizacija JSON-a nije uspjela: {ex.Message}");
            return result;
        }

        var incoming = fromXml.Concat(fromJson).ToList();

        foreach (var calendarEvent in incoming)
            calendarEvent.GoogleEventId = calendarEvent.GoogleEventId.Trim();

        var duplicatesInPayload = incoming
            .GroupBy(e => e.GoogleEventId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var googleEventId in duplicatesInPayload)
            result.BusinessErrors.Add($"Događaj s googleEventId '{googleEventId}' pojavljuje se više puta u poslanim datotekama.");

        var incomingIds = incoming.Select(e => e.GoogleEventId).Distinct().ToList();
        var existingIds = await repository.GetExistingGoogleEventIdsAsync(incomingIds);

        foreach (var googleEventId in existingIds)
            result.BusinessErrors.Add($"Događaj s googleEventId '{googleEventId}' već postoji u bazi.");

        if (result.BusinessErrors.Count > 0)
            return result;

        var entities = incoming.Select(e => e.ToEntity()).ToList();

        await repository.AddRangeAsync(entities);
        await repository.SaveChangesAsync();

        result.ImportedCount = entities.Count;
        return result;
    }

    private static async Task<byte[]> BufferAsync(Stream source, CancellationToken cancellationToken)
    {
        using var memory = new MemoryStream();
        await source.CopyToAsync(memory, cancellationToken);
        return memory.ToArray();
    }

    private static List<ImportCalendarEventDto> DeserializeXml(byte[] buffer)
    {
        var serializer = new XmlSerializer(typeof(ImportCalendarEventsDto));
        using var stream = new MemoryStream(buffer);

        var wrapper = (ImportCalendarEventsDto?)serializer.Deserialize(stream);
        return wrapper?.CalendarEvents ?? [];
    }

    private static List<ImportCalendarEventDto> DeserializeJson(byte[] buffer)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var wrapper = JsonSerializer.Deserialize<ImportCalendarEventsDto>(buffer, options);
        return wrapper?.CalendarEvents ?? [];
    }
}
