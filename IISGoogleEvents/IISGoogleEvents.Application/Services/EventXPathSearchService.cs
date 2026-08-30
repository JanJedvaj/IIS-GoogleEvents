using System.Xml;
using System.Xml.XPath;
using IISGoogleEvents.Application.Dtos.Soap;
using Microsoft.Extensions.Logging;

namespace IISGoogleEvents.Application.Services;

public class EventXPathSearchService(XmlExportService export, ILogger<EventXPathSearchService> logger)
{
    private const string TargetNamespace = "http://iis.algebra.hr/calendar";
    private const string Prefix = "ev";

    public EventSearchResponse Search(string searchTerm)
    {
        var response = new EventSearchResponse { SearchTerm = searchTerm ?? "" };

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            response.Message = "Pojam za pretraživanje ne smije biti prazan.";
            return response;
        }

        var term = searchTerm.Trim();

        XPathDocument document;
        try
        {
            using var stream = export.OpenRead();
            document = new XPathDocument(stream);
        }
        catch (FileNotFoundException)
        {
            response.Message = "XML datoteka nije generirana. Pokreni generiranje prije pretraživanja.";
            return response;
        }

        var navigator = document.CreateNavigator();

        var namespaces = new XmlNamespaceManager(navigator.NameTable);
        namespaces.AddNamespace(Prefix, TargetNamespace);

        var expression = BuildExpression(term);

        logger.LogInformation("XPath izraz: {Expression}", expression);

        var nodes = navigator.Select(expression, namespaces);

        while (nodes.MoveNext())
        {
            var node = nodes.Current!;
            response.Results.Add(ReadEvent(node, namespaces, term));
        }

        response.TotalFound = response.Results.Count;

        if (response.TotalFound == 0)
            response.Message = $"Nije pronađen nijedan događaj za pojam „{term}”.";

        return response;
    }

    public int GetCount()
    {
        try
        {
            using var stream = export.OpenRead();
            var navigator = new XPathDocument(stream).CreateNavigator();

            var namespaces = new XmlNamespaceManager(navigator.NameTable);
            namespaces.AddNamespace(Prefix, TargetNamespace);

            return (int)(double)navigator.Evaluate(
                $"count(/{Prefix}:calendarEvents/{Prefix}:calendarEvent)", namespaces);
        }
        catch (FileNotFoundException)
        {
            return 0;
        }
    }

    private static string BuildExpression(string term)
    {
        var lower = Escape(term.ToLowerInvariant());

        const string Upper = "'ABCDEFGHIJKLMNOPQRSTUVWXYZČĆĐŠŽ'";
        const string Lower = "'abcdefghijklmnopqrstuvwxyzčćđšž'";

        string Contains(string element) =>
            $"contains(translate({Prefix}:{element}, {Upper}, {Lower}), {lower})";

        return $"/{Prefix}:calendarEvents/{Prefix}:calendarEvent[" +
               $"{Contains("summary")} or " +
               $"{Contains("description")} or " +
               $"{Contains("location")}]";
    }

    private static EventSearchResultDto ReadEvent(
        XPathNavigator node, XmlNamespaceManager namespaces, string term)
    {
        var summary = Value(node, "summary", namespaces);
        var description = Value(node, "description", namespaces);
        var location = Value(node, "location", namespaces);

        var matched = new List<string>();
        if (Matches(summary, term)) matched.Add("naziv");
        if (Matches(description, term)) matched.Add("opis");
        if (Matches(location, term)) matched.Add("lokacija");

        return new EventSearchResultDto
        {
            GoogleEventId = Value(node, "googleEventId", namespaces),
            Summary = summary,
            Description = string.IsNullOrEmpty(description) ? null : description,
            Location = string.IsNullOrEmpty(location) ? null : location,
            Start = Value(node, "start", namespaces),
            End = Value(node, "end", namespaces),
            Status = Value(node, "status", namespaces),
            MatchedIn = string.Join(", ", matched)
        };
    }

    private static string Value(XPathNavigator node, string element, XmlNamespaceManager ns) =>
        node.SelectSingleNode($"{Prefix}:{element}", ns)?.Value ?? "";

    private static bool Matches(string value, string term) =>
        !string.IsNullOrEmpty(value) &&
        value.Contains(term, StringComparison.OrdinalIgnoreCase);

    private static string Escape(string value) =>
        value.Contains('\'')
            ? "concat('" + value.Replace("'", "', \"'\", '") + "')"
            : $"'{value}'";
}
