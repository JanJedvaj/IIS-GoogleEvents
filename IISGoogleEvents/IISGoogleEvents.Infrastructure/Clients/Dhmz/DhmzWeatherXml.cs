using System.Xml.Serialization;

namespace IISGoogleEvents.Infrastructure.Clients.Dhmz;

[XmlRoot("Hrvatska")]
public class DhmzWeatherXml
{
    [XmlElement("DatumTermin")]
    public DatumTermin DatumTermin { get; set; } = new();

    [XmlElement("Grad")]
    public List<Grad> Gradovi { get; set; } = [];
}

public class DatumTermin
{
    [XmlElement("Datum")]
    public string Datum { get; set; } = string.Empty;

    [XmlElement("Termin")]
    public string Termin { get; set; } = string.Empty;
}

public class Grad
{
    [XmlAttribute("autom")]
    public string IsAutomatic { get; set; } = string.Empty;

    [XmlElement("GradIme")]
    public string GradIme { get; set; } = string.Empty;

    [XmlElement("Lat")]
    public string Lat { get; set; } = string.Empty;

    [XmlElement("Lon")]
    public string Lon { get; set; } = string.Empty;

    [XmlElement("Podatci")]
    public Podatci Podatci { get; set; } = new();
}

public class Podatci
{
    [XmlElement("Temp")]
    public string Temp { get; set; } = string.Empty;

    [XmlElement("Vlaga")]
    public string Vlaga { get; set; } = string.Empty;

    [XmlElement("Tlak")]
    public string Tlak { get; set; } = string.Empty;

    [XmlElement("TlakTend")]
    public string TlakTend { get; set; } = string.Empty;

    [XmlElement("VjetarSmjer")]
    public string VjetarSmjer { get; set; } = string.Empty;

    [XmlElement("VjetarBrzina")]
    public string VjetarBrzina { get; set; } = string.Empty;

    [XmlElement("Vrijeme")]
    public string Vrijeme { get; set; } = string.Empty;

    [XmlElement("VrijemeZnak")]
    public string VrijemeZnak { get; set; } = string.Empty;
}
