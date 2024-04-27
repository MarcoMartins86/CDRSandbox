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

    public async Task<CdrItem?> FetchItemAsync(string referenceString)
    {
        var reference = new CdrReference(referenceString);
        var entity = await repository.FetchItemAsync(reference.Value);

        return CdrItem.From(entity);
    }
}