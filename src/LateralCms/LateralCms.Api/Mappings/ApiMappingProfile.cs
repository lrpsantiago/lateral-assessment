using LateralCms.Api.Contracts.Requests;
using LateralCms.Application.Services.Contracts;
using LateralCms.Domain.Entities;
using Mapster;

namespace LateralCms.Api.Mappings;

public sealed class ApiMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PostCmsEventRequest, CmsEventInput>()
            .Map(x => x.EntityId, source => source.Id)
            .TwoWays();

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
}
