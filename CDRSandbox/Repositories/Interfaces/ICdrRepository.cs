namespace CDRSandbox.Repositories.Interfaces;

public interface ICdrRepository
{
    public Task<long> StoreAsync(IEnumerable<ICdrItemEntity> items);
    public Task<long> StoreAsync(IEnumerable<object?[]> items);
    public Task<ICdrItemEntity?> FetchItemAsync(string reference);
    public Task<IEnumerable<ICdrItemEntity>> FetchItemsFromCallerAsync(string callerId, DateTime from, DateTime to, int? type);
}