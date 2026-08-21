using LateralCms.Api.Contracts.Requests;
using LateralCms.Application.Services;
using LateralCms.Application.Services.Contracts;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LateralCms.Api.Controllers;

[Authorize(Roles = "admin,user")]
[Route("api/[controller]")]
[ApiController]
public class EntityController : LateralCmsController
{
    private readonly ICmsEntityService _entityService;
    private readonly IMapper _mapper;

    public EntityController(ICmsEntityService entityService, IMapper mapper)
    {
        _entityService = entityService;
        _mapper = mapper;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllEntitiesAsync()
    {
        var entities = await _entityService.GetAllEntitiesAsync();

        return Ok(entities);
    }

    [HttpGet("published")]
    public async Task<IActionResult> GetPublishedEntitiesAsync()
    {
        var publishedEntities = await _entityService.GetPublishedEntitiesAsync();

        return Ok(publishedEntities);
    }

    [HttpPut("visibility")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> SetVisbilityAsync([FromBody] SetEntityVisibilityRequest request)
    {
        var currentUser = User.Identity?.Name;
        await _entityService.SetVisibilityAsync(request.Id, request.IsVisible, currentUser);

        return NoContent();
    }
}
