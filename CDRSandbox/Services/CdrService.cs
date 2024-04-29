using System.Globalization;
using CDRSandbox.Repositories.Interfaces;
using CDRSandbox.Services.Models;
using CDRSandbox.Services.Models.ValueObjects;
using CsvHelper;

namespace CDRSandbox.Services;

public class CdrService(ICdrRepository repository)
{
    public async Task<long> ProcessCsvFileAndStoreAsync(Stream content)
    {
        using var reader = new StreamReader(content, leaveOpen: true); // content is not owned by us, so we shouldn't close it
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture, true); // content is not owned by us, so we shouldn't close it
        
        // Get the items in a lazy way (they will only be evaluated when needed)
        var items = csv.GetRecordsAsync<CdrCsvItem>().ToBlockingEnumerable();
        
        // Call our CDR repository to save them
        return await repository.StoreAsync(items.Select(i => i.ToObjects()));
    }

    public async Task<CdrItem?> FetchRecordAsync(Reference reference)
    {
        var entity = await repository.FetchRecordAsync(reference.ToString());

        return CdrItem.FromOrNull(entity);
    }

    public async Task<IEnumerable<CdrItem>> FetchRecordsAsync(Phone calledId, Date from, Date to, CdrCallTypeEnum? type = null, long? nExpensiveCall = null)
    {
        var items = await repository.FetchRecordsAsync(calledId.ToString(), from.ToDateTime(), to.ToDateTime(), (int?)type, nExpensiveCall);
        
        return items.Select(CdrItem.From);
    }

    public async Task<(long count, long totalDuration)> FetchCountTotalDurationCalls(Date from, Date to, CdrCallTypeEnum? type = null)
    {
        var result = await repository.FetchCountTotalDurationCalls(from.ToDateTime(), to.ToDateTime(), (int?)type);
        return result == null ? (0, 0) : (result.Count, result.TotalDuration);
    }
}