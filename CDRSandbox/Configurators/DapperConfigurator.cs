using CDRSandbox.Configurators.CustomMaps;
using Dapper;
using Dapper.FluentMap;

namespace CDRSandbox.Configurators;

public static class DapperConfigurator
{
    public static void Setup()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        FluentMapper.Initialize(config =>
        {
            // config.ForDommel(); // TODO: check if using this with ColumnNameAttribute has good performance and so we can replace the custom maps
            config.AddMap(new CdrItemClickHouseEntityMap());
        });
    }
}