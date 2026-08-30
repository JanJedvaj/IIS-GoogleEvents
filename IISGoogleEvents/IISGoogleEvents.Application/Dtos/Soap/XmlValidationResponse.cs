using System.Runtime.Serialization;

namespace IISGoogleEvents.Application.Dtos.Soap;

[DataContract(Namespace = "http://iis.algebra.hr/soap")]
public class XmlValidationResponse
{
    [DataMember(Order = 1)]
    public bool IsValid { get; set; }

    [DataMember(Order = 2)]
    public int EventCount { get; set; }

    [DataMember(Order = 3)]
    public List<string> Errors { get; set; } = [];

    [DataMember(Order = 4)]
    public string? Message { get; set; }
}
