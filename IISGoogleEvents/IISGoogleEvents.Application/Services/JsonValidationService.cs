using System.Text.Json;
using Json.Schema;

namespace IISGoogleEvents.Application.Services;

public class JsonValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; } = [];
}

public class JsonValidationService
{
    private readonly JsonSchema _schema;
    private readonly EvaluationOptions _options;

    public JsonValidationService()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Schemas", "calendar-event.schema.json");

        _schema = JsonSchema.FromFile(path);

        _options = new EvaluationOptions
        {
            OutputFormat = OutputFormat.List,
            RequireFormatValidation = true
        };
    }

    public JsonValidationResult Validate(Stream json)
    {
        var result = new JsonValidationResult();

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            result.Errors.Add($"JSON nije dobro oblikovan: {ex.Message}");
            return result;
        }

        using (document)
        {
            var evaluation = _schema.Evaluate(document.RootElement, _options);

            if (evaluation.IsValid)
                return result;

            var details = evaluation.Details ?? [];

            foreach (var detail in details)
            {
                if (detail.Errors is null || detail.Errors.Count == 0)
                    continue;

                var location = detail.InstanceLocation.ToString();

                var hasFailingChildren = details.Any(d =>
                {
                    if (ReferenceEquals(d, detail))
                        return false;

                    if (d.Errors is not { Count: > 0 })
                        return false;

                    var childLocation = d.InstanceLocation.ToString();
                    return childLocation != location && childLocation.StartsWith(location);
                });

                if (hasFailingChildren)
                    continue;

                var displayLocation = location.Length == 0 ? "/" : location;

                foreach (var error in detail.Errors)
                    result.Errors.Add($"{displayLocation}: {error.Value}");
            }

            if (result.Errors.Count == 0)
                result.Errors.Add("Dokument nije valjan prema shemi.");
        }

        return result;
    }
}
