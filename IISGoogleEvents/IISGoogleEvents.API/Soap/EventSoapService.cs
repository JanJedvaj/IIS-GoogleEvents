using System.ServiceModel;
using IISGoogleEvents.Application.Dtos.Soap;
using IISGoogleEvents.Application.Services;
using Microsoft.AspNetCore.Http;

namespace IISGoogleEvents.API.Soap;

public class EventSoapService(
    EventXPathSearchService search,
    XmlExportService export,
    XmlValidationService validator,
    IHttpContextAccessor httpContextAccessor,
    ILogger<EventSoapService> logger) : IEventSoapService
{
    public EventSearchResponse SearchEvents(string searchTerm)
    {
        EnsureAuthenticated();

        logger.LogInformation("SOAP poziv SearchEvents s pojmom: {Term}", searchTerm);
        return search.Search(searchTerm);
    }

    public int GetEventCount()
    {
        EnsureAuthenticated();

        return search.GetCount();
    }

    public XmlValidationResponse ValidateGeneratedXml()
    {
        EnsureAuthenticated();

        var response = new XmlValidationResponse();

        try
        {
            using var stream = export.OpenRead();
            var result = validator.Validate(stream);

            response.IsValid = result.IsValid;
            response.Errors.AddRange(result.Errors);
            response.EventCount = search.GetCount();

            response.Message = result.IsValid
                ? "Generirana XML datoteka je valjana prema XSD shemi."
                : $"Pronađeno {result.Errors.Count} pogrešaka validacije.";
        }
        catch (FileNotFoundException)
        {
            response.Message = "XML datoteka nije generirana.";
        }

        return response;
    }

    private void EnsureAuthenticated()
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
            throw new FaultException("Za pozivanje ove SOAP operacije potreban je valjani JWT token.");
    }
}
