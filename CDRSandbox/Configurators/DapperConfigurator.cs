using Dapper;

namespace CDRSandbox.Configurators;

public static class DapperConfigurator
{
    public static void Setup()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }
}