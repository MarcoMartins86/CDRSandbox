using CDRSandbox.Services.Models;

namespace CDRSandbox.Repositories.Interfaces;

public interface ICdrRepository
{
    public Task<long> Store(IEnumerable<CdrItem> items);
}