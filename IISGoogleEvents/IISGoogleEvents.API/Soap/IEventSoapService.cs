using System.ServiceModel;
using IISGoogleEvents.Application.Dtos.Soap;

namespace IISGoogleEvents.API.Soap;

[ServiceContract(Namespace = "http://iis.algebra.hr/soap")]
public interface IEventSoapService
{
    [OperationContract]
    EventSearchResponse SearchEvents(string searchTerm);

    [OperationContract]
    int GetEventCount();

    [OperationContract]
    XmlValidationResponse ValidateGeneratedXml();
}
