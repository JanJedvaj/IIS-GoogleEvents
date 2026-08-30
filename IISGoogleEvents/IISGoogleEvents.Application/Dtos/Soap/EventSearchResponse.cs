using System.Runtime.Serialization;

namespace IISGoogleEvents.Application.Dtos.Soap;

[DataContract(Namespace = "http://iis.algebra.hr/soap")]
public class EventSearchResponse
{
    [DataMember(Order = 1)]
    public string SearchTerm { get; set; } = "";

    [DataMember(Order = 2)]
    public int TotalFound { get; set; }

    [DataMember(Order = 3)]
    public List<EventSearchResultDto> Results { get; set; } = [];

    [DataMember(Order = 4)]
    public string? Message { get; set; }
}
