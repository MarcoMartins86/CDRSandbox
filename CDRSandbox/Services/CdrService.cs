using System.Globalization;
using CDRSandbox.Helpers;
using CDRSandbox.Repositories.Interfaces;
using CDRSandbox.Services.Models;
using CsvHelper;

namespace CDRSandbox.Services;

public class CdrService(ICdrRepository repository)
{
    public async Task<long> ProcessCsvFileAndStoreData(Stream content)
    {
        using var reader = new StreamReader(content, leaveOpen: true); // content is not owned by us, so we shouldn't close it
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture, true); // content is not owned by us, so we shouldn't close it
        // Add our converter of byte[] to pad the data if needed
        csv.Context.TypeConverterCache.AddConverter<byte[]>(new PaddedByteArrayConverterHelper());
        // Get the items in a lazy way (they will only be evaluated when needed)
        var items = csv.GetRecordsAsync<CdrItem>().ToBlockingEnumerable();
        // Call our CDR repository to save them
        return await repository.Store(items);
    }
}