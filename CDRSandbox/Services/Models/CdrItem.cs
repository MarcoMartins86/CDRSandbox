using CDRSandbox.Repositories.Interfaces;
using CDRSandbox.Services.Models.ValueObjects;

namespace CDRSandbox.Services.Models;

public class CdrItem
{
    public const string CallDateFormat = "dd/MM/yyyy";
    public const string EndTimeFormat = "HH:mm:ss";
    public const string ReferencePattern = "^[0-9a-fA-F]{1,33}$";
    
    public Phone CallerId { get; set; }
    public Phone Recipient { get; set; }
    public Date CallDate { get; set; }
    public Time EndTime { get; set; }
    public Span Duration { get; set; }
    public Money Cost { get; set; }
    public CdrReference Reference { get; set; }
    public CdrCallTypeEnum? Type { get; set; }

    public static CdrItem? From(ICdrItemEntity? entity)
    {
        return entity != null ? new CdrItem()
        {
            CallerId = new Phone(entity.CallerId),
            Recipient = new Phone(entity.Recipient),
            CallDate = new Date(entity.CallDate.ToString(CallDateFormat)),
            EndTime = new Time(entity.EndTime),
            Duration = new Span(entity.Duration),
            Cost = new Money(entity.Cost, entity.Currency),
            Reference = new CdrReference(entity.Reference),
            Type = (CdrCallTypeEnum?)entity.Type, // TODO: improve
        } : null;
    }
}