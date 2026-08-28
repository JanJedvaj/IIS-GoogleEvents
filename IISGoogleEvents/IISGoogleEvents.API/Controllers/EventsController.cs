using IISGoogleEvents.API.Authorization;
using IISGoogleEvents.Application.Dtos.Events;
using IISGoogleEvents.Application.Interfaces;
using IISGoogleEvents.Application.Models;
using IISGoogleEvents.Infrastructure.Enums;
using Microsoft.AspNetCore.Mvc;

namespace IISGoogleEvents.API.Controllers;

[AuthorizeRoles(MinRole = Roles.User)]
[Produces("application/json")]
public class EventsController : BaseController
{
    private readonly ICalendarEventService _calendarEventService;

    public EventsController(ICalendarEventService calendarEventService)
    {
        _calendarEventService = calendarEventService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(StandardResponse<IEnumerable<CalendarEventDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search([FromQuery] string? query = null)
    {
        var response = await _calendarEventService.SearchAsync(query);
        return HandleResponse(response);
    }

    [HttpGet("capabilities")]
    [ProducesResponseType(typeof(StandardResponse<CalendarCapabilitiesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Capabilities() =>
        HandleResponse(StandardResponse<CalendarCapabilitiesDto>.Create(
            ResultStatus.Ok,
            _calendarEventService.Capabilities));

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StandardResponse<CalendarEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(StandardResponse<CalendarEventDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        var response = await _calendarEventService.GetAsync(id);
        return HandleResponse(response);
    }

    [AuthorizeRoles(Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(StandardResponse<CalendarEventDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(StandardResponse<CalendarEventDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateCalendarEventDto request)
    {
        var response = await _calendarEventService.CreateAsync(request);
        return HandleResponse(response);
    }

    [AuthorizeRoles(Roles.Admin)]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(StandardResponse<CalendarEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(StandardResponse<CalendarEventDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateCalendarEventDto request)
    {
        var response = await _calendarEventService.UpdateAsync(id, request);
        return HandleResponse(response);
    }

    [AuthorizeRoles(Roles.Admin)]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(StandardResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(StandardResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        var response = await _calendarEventService.DeleteAsync(id);
        return HandleResponse(response);
    }
}
