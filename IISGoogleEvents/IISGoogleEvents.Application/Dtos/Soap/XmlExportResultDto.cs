namespace IISGoogleEvents.Application.Dtos.Soap;

public class XmlExportResultDto
{
    public string FilePath { get; set; } = "";
    public int EventCount { get; set; }
    public DateTimeOffset GeneratedAt { get; set; }
}
