using System.Runtime.Serialization;

namespace IISGoogleEvents.Application.Dtos.Soap;

[DataContract(Namespace = "http://iis.algebra.hr/soap")]
public class EventSearchResultDto
{
    [DataMember(Order = 1)]
    public string GoogleEventId { get; set; } = "";

    [DataMember(Order = 2)]
    public string Summary { get; set; } = "";

    [DataMember(Order = 3)]
    public string? Description { get; set; }

    [DataMember(Order = 4)]
    public string? Location { get; set; }

    [DataMember(Order = 5)]
    public string Start { get; set; } = "";

    [DataMember(Order = 6)]
    public string End { get; set; } = "";

    [DataMember(Order = 7)]
    public string Status { get; set; } = "";

    [DataMember(Order = 8)]
    public string MatchedIn { get; set; } = "";
}
