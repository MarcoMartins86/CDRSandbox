namespace CDRSandbox.Repositories.Interfaces;

public interface ICdrRepository
{
    public Task<long> StoreAsync(IEnumerable<ICdrItemEntity> items);
    public Task<long> StoreAsync(IEnumerable<object?[]> items);
    public Task<ICdrItemEntity?> FetchRecordAsync(string reference);
    public Task<IEnumerable<ICdrItemEntity>> FetchRecordsAsync(string callerId, DateTime from, DateTime to, int? type = null, long? nExpensiveCall = null);
    public Task<ICdrCountTotalDuration?> FetchCountTotalDurationCalls(DateTime from, DateTime to, int? type = null);
}