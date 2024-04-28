using CDRSandbox.Repositories.Interfaces;

namespace CDRSandbox.Repositories.ClickHouse.Projections;

public class CdrCountTotalDuration : ICdrCountTotalDuration
{
    public long Count { get; set; }
    public long TotalDuration { get; set; }
}