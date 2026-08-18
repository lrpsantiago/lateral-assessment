using LateralCms.Api.Contracts.Requests;
using LateralCms.Application.Services;
using LateralCms.Application.Services.Contracts;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LateralCms.Api.Controllers;

[Authorize(Roles = "cms")]
[Route("api/[controller]")]
[ApiController]
public class CmsController : LateralCmsController
{
    private readonly ICmsEventService _eventService;
    private readonly IMapper _mapper;

    public CmsController(
        ICmsEventService eventService,
        IMapper mapper)
    {
        _eventService = eventService;
        _mapper = mapper;
    }

    [HttpPost("events")]
    public async Task<IActionResult> PostEventsAsync([FromBody] PostCmsEventRequest[] events, CancellationToken cancellationToken)
    {
        if (events == null)
        {
            return BadRequest("Event array required.");
        }

        if (events.Length <= 0)
        {
            return BadRequest("At least one event is required.");
        }

        var eventList = events?
            .Select(_mapper.Map<CmsEventInput>)
            .ToList();

        var output = await _eventService.ReceiveAsync(eventList!, cancellationToken);

        return Accepted(output);
    }
}
