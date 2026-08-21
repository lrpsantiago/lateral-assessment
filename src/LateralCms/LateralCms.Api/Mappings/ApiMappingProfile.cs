using LateralCms.Api.Contracts.Requests;
using LateralCms.Api.Controllers;
using LateralCms.Application.Services.Contracts;
using LateralCms.Domain.Entities;
using Mapster;
using System.Text.Json;

namespace LateralCms.Api.Mappings;

public sealed class ApiMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PostCmsEventRequest, CmsEventInput>()
            .Map(destination => destination.EntityId, source => source.Id)
            .Map(destination => destination.Payload, source => SerializePayload(source.Payload));

        config.NewConfig<UpdateCmsEntityRequest, EntityPayloadUpdateInput>()
            .Map(x => x.EntityId, source => source.Id)
            .TwoWays();

        config.NewConfig<CmsEvent, CmsEntityInput>()
            .Map(x => x.Id, source => source.EntityId)
            .Map(x => x.Payload, source => source.Payload)
            .TwoWays();

        config.NewConfig<CmsEvent, EntityPayloadUpdateInput>()
            .Map(x => x.EntityId, source => source.EntityId)
            .Map(x => x.Payload, source => source.Payload)
            .TwoWays();
    }

    private static string? SerializePayload(JsonElement? payload)
    {
        if (!payload.HasValue
            || payload.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return JsonSerializer.Serialize(payload.Value);
    }
}
