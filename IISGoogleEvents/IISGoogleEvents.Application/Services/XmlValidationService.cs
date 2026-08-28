using System.Xml;
using System.Xml.Schema;

namespace IISGoogleEvents.Application.Services;

public class XmlValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; } = [];
}

public class XmlValidationService
{
    private const string TargetNamespace = "http://iis.algebra.hr/calendar";

    private readonly XmlSchemaSet _schemas;

    public XmlValidationService()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Schemas", "calendar-event.xsd");

        _schemas = new XmlSchemaSet();

        using (var reader = XmlReader.Create(path))
        {
            _schemas.Add(TargetNamespace, reader);
        }

        _schemas.Compile();
    }

    public XmlValidationResult Validate(Stream xml)
    {
        var result = new XmlValidationResult();

        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            Schemas = _schemas,
            DtdProcessing = DtdProcessing.Prohibit
        };

        settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

        settings.ValidationEventHandler += (_, e) =>
        {
            var line = e.Exception?.LineNumber ?? 0;
            var position = e.Exception?.LinePosition ?? 0;
            result.Errors.Add($"Redak {line}, pozicija {position}: {e.Message}");
        };

        try
        {
            using var reader = XmlReader.Create(xml, settings);
            while (reader.Read())
            {
            }
        }
        catch (XmlException ex)
        {
            result.Errors.Add($"XML nije dobro oblikovan - redak {ex.LineNumber}: {ex.Message}");
        }

        return result;
    }
}
