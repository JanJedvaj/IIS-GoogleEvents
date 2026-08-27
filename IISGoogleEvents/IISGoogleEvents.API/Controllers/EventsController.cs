using IISGoogleEvents.API.Abstractions.Attributes;
using IISGoogleEvents.API.Abstractions.Controllers;
using IISGoogleEvents.Application.DTOs.Events;
using IISGoogleEvents.Application.Interfaces.Services;
using IISGoogleEvents.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace IISGoogleEvents.API.Controllers;

/// <summary>
/// Requirement 5's REST surface. Every action delegates straight to
/// ICalendarEventService, so this controller is agnostic of Local vs External.
/// </summary>
[AuthorizeRoles(MinRole = Roles.User)]
public class EventsController : BaseController
{
    private readonly ICalendarEventService _calendarEventService;

    public EventsController(ICalendarEventService calendarEventService)
    {
        _calendarEventService = calendarEventService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? query = null)
    {
        var response = await _calendarEventService.SearchAsync(query);
        return HandleResponse(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        var response = await _calendarEventService.GetAsync(id);
        return HandleResponse(response);
    }

    [AuthorizeRoles(Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCalendarEventDto request)
    {
        var response = await _calendarEventService.CreateAsync(request);
        return HandleResponse(response);
    }

    [AuthorizeRoles(Roles.Admin)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateCalendarEventDto request)
    {
        var response = await _calendarEventService.UpdateAsync(id, request);
        return HandleResponse(response);
    }

    [AuthorizeRoles(Roles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        var response = await _calendarEventService.DeleteAsync(id);
        return HandleResponse(response);
    }
}
