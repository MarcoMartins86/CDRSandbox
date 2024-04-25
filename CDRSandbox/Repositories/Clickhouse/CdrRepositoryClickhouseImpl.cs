using CDRSandbox.Repositories.Interfaces;
using Microsoft.Extensions.Options;

namespace CDRSandbox.Repositories.Clickhouse;

public class CdrRepositoryClickhouseImpl : ICdrRepository, IDisposable
{
    public const string TableName = "call_detail_record";
    public CdrRepositoryClickhouseImpl(IOptions<DbOptionsClickhouse> options)
    {
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}