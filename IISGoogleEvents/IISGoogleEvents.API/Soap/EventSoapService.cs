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
                : $"Pronađeno {result.Errors.Count} {PluralizeErrors(result.Errors.Count)} validacije.";
        }
        catch (FileNotFoundException)
        {
            response.Message = "XML datoteka nije generirana.";
        }

        return response;
    }

    private static string PluralizeErrors(int count)
    {
        var lastTwo = count % 100;
        var last = count % 10;

        if (last == 1 && lastTwo != 11) return "pogreška";
        if (last is >= 2 and <= 4 && lastTwo is < 12 or > 14) return "pogreške";
        return "pogrešaka";
    }

    private void EnsureAuthenticated()
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
            throw new FaultException("Za pozivanje ove SOAP operacije potreban je valjani JWT token.");
    }
}
