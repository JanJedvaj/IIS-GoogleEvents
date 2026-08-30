using System.Globalization;
using System.Text;
using System.Xml;
using IISGoogleEvents.Application.Dtos.Events;
using IISGoogleEvents.Application.Dtos.Soap;

namespace IISGoogleEvents.Application.Services;

public class XmlExportService(LocalCalendarEventService calendarEventService, string generatedDirectory)
{
    private const string TargetNamespace = "http://iis.algebra.hr/calendar";

    public string ExportFilePath => Path.Combine(generatedDirectory, "events.xml");

    public async Task<XmlExportResultDto> GenerateAsync(CancellationToken ct = default)
    {
        var response = await calendarEventService.SearchAsync(null);
        var events = (response.Data ?? []).ToList();

        var path = ExportFilePath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = new UTF8Encoding(false),
            Async = false
        };

        await using var file = File.Create(path);
        using var writer = XmlWriter.Create(file, settings);

        writer.WriteStartDocument();
        writer.WriteStartElement("calendarEvents", TargetNamespace);

        foreach (var calendarEvent in events)
            WriteEvent(writer, calendarEvent);

        writer.WriteEndElement();
        writer.WriteEndDocument();

        return new XmlExportResultDto
        {
            FilePath = path,
            EventCount = events.Count,
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }

    public Stream OpenRead()
    {
        var path = ExportFilePath;

        if (!File.Exists(path))
            throw new FileNotFoundException(
                "XML datoteka još nije generirana. Pozovi generiranje prije čitanja.", path);

        return File.OpenRead(path);
    }

    private static void WriteEvent(XmlWriter writer, CalendarEventDto calendarEvent)
    {
        writer.WriteStartElement("calendarEvent", TargetNamespace);

        writer.WriteElementString("googleEventId", TargetNamespace, calendarEvent.GoogleEventId);
        writer.WriteElementString("summary", TargetNamespace, calendarEvent.Summary);

        if (!string.IsNullOrEmpty(calendarEvent.Description))
            writer.WriteElementString("description", TargetNamespace, calendarEvent.Description);

        if (!string.IsNullOrEmpty(calendarEvent.Location))
            writer.WriteElementString("location", TargetNamespace, calendarEvent.Location);

        writer.WriteElementString("start", TargetNamespace, calendarEvent.Start.ToString("o", CultureInfo.InvariantCulture));
        writer.WriteElementString("end", TargetNamespace, calendarEvent.End.ToString("o", CultureInfo.InvariantCulture));

        writer.WriteElementString("isAllDay", TargetNamespace, XmlConvert.ToString(calendarEvent.IsAllDay));

        if (!string.IsNullOrEmpty(calendarEvent.Status))
            writer.WriteElementString("status", TargetNamespace, calendarEvent.Status);

        if (!string.IsNullOrEmpty(calendarEvent.HtmlLink))
            writer.WriteElementString("htmlLink", TargetNamespace, calendarEvent.HtmlLink);

        writer.WriteElementString("created", TargetNamespace, calendarEvent.Created.ToString("o", CultureInfo.InvariantCulture));
        writer.WriteElementString("updated", TargetNamespace, calendarEvent.Updated.ToString("o", CultureInfo.InvariantCulture));

        writer.WriteEndElement();
    }
}
