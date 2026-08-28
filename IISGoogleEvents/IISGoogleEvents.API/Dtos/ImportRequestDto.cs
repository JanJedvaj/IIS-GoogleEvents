namespace IISGoogleEvents.API.Dtos;

public class ImportRequestDto
{
    public IFormFile? XmlFile { get; set; }
    public IFormFile? JsonFile { get; set; }
}
