using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LensmaniaLibrary.DTOs.Events;
using LensmaniaServer.Services;

namespace LensmaniaServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private const int MaxLimit = 100;

    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    // GET api/events?offset=0&limit=10&status=active
    [HttpGet]
    public async Task<ActionResult<PaginatedEvents>> GetAll(
        [FromQuery] int offset = 0,
        [FromQuery] int limit  = 10,
        [FromQuery] string? status = null)
    {
        if (offset < 0)
            return BadRequest(new { message = "offset doit être supérieur ou égal à 0." });

        if (limit <= 0 || limit > MaxLimit)
            return BadRequest(new { message = $"limit doit être compris entre 1 et {MaxLimit}." });

        var result = await _eventService.GetAllAsync(offset, limit, status);
        return Ok(result);
    }

    // GET api/events/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<EventDetailedResponse>> GetById(int id)
    {
        var ev = await _eventService.GetByIdAsync(id);

        if (ev is null)
            return NotFound();

        return Ok(ev);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<EventDetailedResponse>> Create([FromBody] CreateEventRequest request)
    {
        try
        {
            var ev = await _eventService.CreateAsync(request);
            return Ok(ev);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<EventDetailedResponse>> Update(int id, [FromBody] UpdateEventRequest request)
    {
        try
        {
            var ev = await _eventService.UpdateAsync(id, request);
            if (ev is null) return NotFound();
            return Ok(ev);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _eventService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    // PUT api/events/{id}/cover
    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:int}/cover")]
    public async Task<ActionResult<EventDetailedResponse>> SetCoverPhoto(int id, [FromBody] SetCoverPhotoRequest request)
    {
        var ev = await _eventService.SetCoverPhotoAsync(id, request.PostId);
        if (ev is null)
            return NotFound(new { message = "Événement introuvable ou le post n'appartient pas à cet événement." });
        return Ok(ev);
    }
}
