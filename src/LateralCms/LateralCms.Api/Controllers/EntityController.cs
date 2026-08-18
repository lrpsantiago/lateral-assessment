using LateralCms.Api.Contracts.Requests;
using LateralCms.Application.Services;
using LateralCms.Application.Services.Contracts;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LateralCms.Api.Controllers;

[Authorize(Roles = "admin")]
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
    public async Task<IActionResult> GetAllEntitiesAsync()
    {
        var entities = await _entityService.GetAllEntitiesAsync();

        return Ok(entities);
    }

    [Authorize(Roles = "admin,user")]
    [HttpGet("published")]
    public async Task<IActionResult> GetPublishedEntitiesAsync()
    {
        var publishedEntities = await _entityService.GetPublishedEntitiesAsync();

        return Ok(publishedEntities);
    }

    [HttpPost]
    public async Task<IActionResult> AddEntityAsync([FromBody] AddCmsEntityRequest request)
    {
        var input = _mapper.Map<CmsEntityInput>(request);
        var output = await _entityService.AddEntityAsync(input);

        return Ok(output);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateEntityAsync([FromBody] UpdateCmsEntityRequest request)
    {
        var input = _mapper.Map<EntityPayloadUpdateInput>(request);
        var output = await _entityService.UpdateEntityAsync(input);

        return Ok(output);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteEntityAsync([FromQuery] string? id)
    {
        await _entityService.DeleteEntityAsync(id);

        return NoContent();
    }

    [HttpPatch("publish")]
    public async Task<IActionResult> PublishEntityAsync([FromQuery] string? id, [FromQuery] int? version = null)
    {
        await _entityService.PublishEntityAsync(id, version);

        return NoContent();
    }

    [HttpPatch("unpublish")]
    public async Task<IActionResult> UnpublishEntityAsync([FromQuery] string? id)
    {
        await _entityService.UnpublishEntityAsync(id);

        return NoContent();
    }
}
