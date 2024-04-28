using CDRSandbox.Services.Models;
using CDRSandbox.Services.Models.ValueObjects;
using RandomDataGenerator.FieldOptions;
using RandomDataGenerator.Randomizers;

namespace CDRSandboxTests.Base;

public class RandomDataGeneratorsBase
{
    private readonly IRandomizerString _randomizerPhoneNumber =
        RandomizerFactory.GetRandomizer(new FieldOptionsTextRegex
            { UseNullValues = false, Pattern = CdrItem.PhoneNumberPattern });

    private readonly IRandomizerDateTime _randomizerDateTime = RandomizerFactory.GetRandomizer(
        new FieldOptionsDateTime()
            { UseNullValues = false, From = DateTime.MinValue, To = DateTime.MaxValue, IncludeTime = true });

    private readonly IRandomizerTimeSpan _randomizerTimeSpan = RandomizerFactory.GetRandomizer(
        new FieldOptionsTimeSpan()
            { UseNullValues = false, From = TimeSpan.Zero, To = TimeSpan.MaxValue, IncludeMilliseconds = false });

    private readonly IRandomizerNumber<double> _randomizerCost = RandomizerFactory.GetRandomizer(
        new FieldOptionsDouble()
            { UseNullValues = false, Min = 0.000d, Max = 5.000d });

    private readonly IRandomizerString _randomizerCurrency =
        RandomizerFactory.GetRandomizer(new FieldOptionsStringList()
            { UseNullValues = false, Values = Currency.ActiveCurrencyArray.ToList() });
    
    private readonly IRandomizerString _randomizerReference =
        RandomizerFactory.GetRandomizer(new FieldOptionsTextRegex
            { UseNullValues = false, Pattern = CdrItem.ReferencePattern });

    private readonly IRandomizerNumber<int> _randomizerType = RandomizerFactory.GetRandomizer(
        new FieldOptionsInteger()
        {
            UseNullValues = false, Min = (int)((CdrCallTypeEnum[])Enum.GetValues(typeof(CdrCallTypeEnum))).First(),
            Max = (int)((CdrCallTypeEnum[])Enum.GetValues(typeof(CdrCallTypeEnum))).Last() + 1
        });
    
    protected string RandomPhoneNumber => _randomizerPhoneNumber.Generate()!;
    protected DateTime RandomDateTime => _randomizerDateTime.Generate()!.Value;
    protected string RandomTime => _randomizerDateTime.Generate()!.Value.ToString(CdrItem.EndTimeFormat);
    protected uint RandomDuration => (uint)_randomizerTimeSpan.Generate()!.Value.TotalSeconds;
    protected double RandomCost => _randomizerCost.Generate()!.Value;
    protected string RandomCurrency => _randomizerCurrency.Generate()!;
    protected string RandomReference => _randomizerReference.Generate()!;
    protected CdrCallTypeEnum RandomType => (CdrCallTypeEnum)_randomizerType.Generate()!;
}