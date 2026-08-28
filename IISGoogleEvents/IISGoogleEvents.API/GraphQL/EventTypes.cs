using HotChocolate;
using IISGoogleEvents.Application.Dtos.Events;
using IISGoogleEvents.Application.Models;

namespace IISGoogleEvents.API.GraphQL;

public record CreateEventInput(
    string Summary,
    string? Description,
    string? Location,
    DateTimeOffset Start,
    DateTimeOffset End,
    bool IsAllDay);

public record UpdateEventInput(
    string? Summary,
    string? Description,
    string? Location,
    DateTimeOffset? Start,
    DateTimeOffset? End,
    bool? IsAllDay);

public static class EventInputMapper
{
    public static CreateCalendarEventDto ToDto(this CreateEventInput input) =>
        new()
        {
            Summary = input.Summary,
            Description = input.Description,
            Location = input.Location,
            Start = input.Start,
            End = input.End,
            IsAllDay = input.IsAllDay
        };

    public static UpdateCalendarEventDto ToDto(this UpdateEventInput input) =>
        new()
        {
            Summary = input.Summary,
            Description = input.Description,
            Location = input.Location,
            Start = input.Start,
            End = input.End,
            IsAllDay = input.IsAllDay
        };
}

public static class StandardResponseExtensions
{
    public static T UnwrapOrThrow<T>(this StandardResponse<T> response)
    {
        if (response.Success)
            return response.Data!;

        throw new GraphQLException(
            ErrorBuilder.New()
                .SetMessage(response.Message ?? "Događaj nije nađen.")
                .SetCode(response.Status == ResultStatus.NotFound ? "NOT_FOUND" : "INTERNAL_ERROR")
                .Build());
    }

    public static T? UnwrapOrDefault<T>(this StandardResponse<T> response) where T : class
    {
        if (response.Success)
            return response.Data;

        if (response.Status == ResultStatus.NotFound)
            return null;

        throw new GraphQLException(
            ErrorBuilder.New()
                .SetMessage(response.Message ?? "Greška poslužitelja.")
                .SetCode("INTERNAL_ERROR")
                .Build());
    }
}
