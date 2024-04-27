using CDRSandbox.Repositories.ClickHouse;
using CDRSandbox.Repositories.ClickHouse.Entities;
using Dapper.FluentMap.Mapping;

namespace CDRSandbox.Configurators.CustomMaps;

public class CdrItemClickHouseEntityMap : EntityMap<CdrItemClickHouseEntity>
{
    public CdrItemClickHouseEntityMap()
    {
        Map(p => p.CallerId)
            .ToColumn(CdrRepositoryClickHouse.CallerIdUnpadColumn);
        Map(p => p.Recipient)
            .ToColumn(CdrRepositoryClickHouse.RecipientUnpadColumn);
        Map(p => p.Reference)
            .ToColumn(CdrRepositoryClickHouse.ReferenceColumnUnpadColumn);
    }
}