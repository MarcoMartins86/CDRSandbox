using System.Collections.Concurrent;
using CDRSandbox.Services.Models;
using CDRSandbox.Services.Models.ValueObjects;
using RandomDataGenerator.FieldOptions;
using RandomDataGenerator.Randomizers;

namespace CDRSandboxTests.Base;

public abstract class RandomDataGeneratorsBase
{
    private static readonly IRandomizerString RandomizerPhoneNumber =
        RandomizerFactory.GetRandomizer(new FieldOptionsTextRegex
            { UseNullValues = false, Pattern = CdrItem.PhoneNumberPattern });

    private static readonly IRandomizerDateTime RandomizerDateTime = RandomizerFactory.GetRandomizer(
        new FieldOptionsDateTime()
        {
            UseNullValues = false, From = DateTime.UtcNow - TimeSpan.FromDays(62) /* 2 months */,
            To = DateTime.UtcNow, IncludeTime = true
        });

    private static readonly IRandomizerTimeSpan RandomizerTimeSpan = RandomizerFactory.GetRandomizer(
        new FieldOptionsTimeSpan()
        {
            UseNullValues = false, From = TimeSpan.Zero, To = new TimeSpan(1, 0, 0, 0), IncludeMilliseconds = false
        });

    private static readonly IRandomizerNumber<float> RandomizerCost = RandomizerFactory.GetRandomizer(
        new FieldOptionsFloat()
            { UseNullValues = false, Min = 0.000f, Max = 5.000f });

    private static readonly IRandomizerString RandomizerCurrency =
        RandomizerFactory.GetRandomizer(new FieldOptionsStringList()
            { UseNullValues = false, Values = Currency.ActiveCurrencyArray.ToList() });

    private static readonly IRandomizerString RandomizerReference =
        RandomizerFactory.GetRandomizer(new FieldOptionsTextRegex
            { UseNullValues = false, Pattern = CdrItem.ReferencePattern });

    private static readonly IRandomizerNumber<int> RandomizerType = RandomizerFactory.GetRandomizer(
        new FieldOptionsInteger()
        {
            UseNullValues = false, Min = (int)((CdrCallTypeEnum[])Enum.GetValues(typeof(CdrCallTypeEnum))).First(),
            Max = (int)((CdrCallTypeEnum[])Enum.GetValues(typeof(CdrCallTypeEnum))).Last() + 1
        });

    private static readonly IRandomizerNumber<int> RandomizerTypeOrNull = RandomizerFactory.GetRandomizer(
        new FieldOptionsInteger()
        {
            UseNullValues = true, Min = (int)((CdrCallTypeEnum[])Enum.GetValues(typeof(CdrCallTypeEnum))).First(),
            Max = (int)((CdrCallTypeEnum[])Enum.GetValues(typeof(CdrCallTypeEnum))).Last() + 1
        });

    // to be used only as a concurrent hash set
    private static readonly ConcurrentDictionary<string, byte> UniqueReferenceConstrainer = new();
    private static readonly ConcurrentDictionary<string, byte> UniqueCallerIdConstrainer = new();

    protected readonly Random Random = new();
    protected string RandomPhoneNumber =>
        EnsureRandomUniqueness(() => RandomizerPhoneNumber.Generate()!, UniqueCallerIdConstrainer);
    protected DateTime RandomDateTime => RandomizerDateTime.Generate()!.Value;
    protected string RandomTime => RandomizerDateTime.Generate()!.Value.ToString(CdrItem.EndTimeFormat);
    protected uint RandomDuration => (uint)RandomizerTimeSpan.Generate()!.Value.TotalSeconds;
    protected float RandomCost => RandomizerCost.Generate()!.Value;
    protected string RandomCurrency => RandomizerCurrency.Generate()!;
    protected string RandomReference =>
        EnsureRandomUniqueness(() => RandomizerReference.Generate()!, UniqueReferenceConstrainer);
    protected CdrCallTypeEnum RandomType => (CdrCallTypeEnum)RandomizerType.Generate()!;
    protected CdrCallTypeEnum? RandomTypeOrNull => (CdrCallTypeEnum?)RandomizerTypeOrNull.Generate();

    private T EnsureRandomUniqueness<T>(Func<T> generator, ConcurrentDictionary<T, byte> uniqueConstrainer) where T : class
    {
        // Generate only unique references, don't rely on random nature for that
        T random;
        while (!uniqueConstrainer.TryAdd(random = generator(), 0))
        {
        }
        return random; 
    }
    
    protected CdrItem RandomCdrItem(string? callerId = null, string? recipient = null, DateTime? callDate = null,
        string? endTime = null, uint? duration = null, float? cost = null, string? currency = null,
        string? reference = null, CdrCallTypeEnum? type = null)
    {
        return new()
        {
            CallerId = new Phone(callerId ?? RandomPhoneNumber),
            Recipient = new Phone(recipient ?? RandomPhoneNumber),
            CallDate = new Date(callDate ?? RandomDateTime),
            EndTime = new Time(endTime ?? RandomTime),
            Duration = new Span(duration ?? RandomDuration),
            Cost = new Money(cost ?? RandomCost, currency ?? RandomCurrency),
            Reference = new Reference(reference ?? RandomReference),
            Type = type ?? RandomTypeOrNull
        };
    }

    protected IEnumerable<CdrItem> RandomCdrItems(int numberItems, int cdrPerCaller = 1)
    {
        var uniqueCallerIdConstrain = new HashSet<string>();
        var result = new List<CdrItem>();
        // Ceiling because if division would get a remainder generated items would be less than numberItems
        for (int i = 0; i < (int)Math.Ceiling((double)numberItems / (double)cdrPerCaller); i++)
        {
            // make sure we use the same unique caller id
            var callerId = RandomPhoneNumber;
            for (int j = 0; j < cdrPerCaller; j++)
            {
                result.Add(RandomCdrItem(callerId));
            }
        }

        // here we truncate the last extra elements, so sometimes the last caller id will have a fewer CDR 
        // remove them at the end for performance
        var divisionRemainder = numberItems % cdrPerCaller;
        if(divisionRemainder != 0)
            result.RemoveRange(result.Count - 1 - divisionRemainder, divisionRemainder); 
        
        return result; 
    }

    private CdrCsvItem CdrItem2CdrCsvItem(CdrItem item) => new()
    {
        CallerId = item.CallerId.ToString(),
        Recipient = item.Recipient.ToString(),
        CallDate = item.CallDate.ToDateOnly(),
        EndTime = item.EndTime.ToTimeOnly(),
        Duration = item.Duration.TotalSeconds,
        Cost = item.Cost.Amount,
        Currency = item.Cost.Currency.ToString(),
        Reference = item.Reference.ToString(),
        Type = item.Type
    };

    protected IEnumerable<CdrCsvItem> RandomCdrCsvItems(int numberItems, int cdrPerCaller = 1) =>
        RandomCdrItems(numberItems, cdrPerCaller).Select(CdrItem2CdrCsvItem);

    protected CdrCsvItem RandomCdrCsvItem(string? callerId = null, string? recipient = null, DateTime? callDate = null,
        string? endTime = null, uint? duration = null, float? cost = null, string? currency = null,
        string? reference = null, CdrCallTypeEnum? type = null) =>
        CdrItem2CdrCsvItem(RandomCdrItem(callerId, recipient, callDate, endTime, duration, cost, currency, reference, type));

    protected T GenerateDistinctRandom<T>(Func<T> generator, T[] existentValues)
    {
        T newValue;

        while (existentValues.Contains(newValue = generator()))
        {
        }

        return newValue;
    }
}