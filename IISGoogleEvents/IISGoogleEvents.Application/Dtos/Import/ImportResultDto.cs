namespace IISGoogleEvents.Application.Dtos.Import;

public class ImportResultDto
{
    public int ImportedCount { get; set; }
    public List<string> XmlErrors { get; set; } = [];
    public List<string> JsonErrors { get; set; } = [];
    public List<string> BusinessErrors { get; set; } = [];
}
