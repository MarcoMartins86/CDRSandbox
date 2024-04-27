using CDRSandbox.Repositories.Interfaces;
using CDRSandbox.Services.Models.ValueObjects;

namespace CDRSandbox.Services.Models;

public class CdrItem
{
    public const string DatePattern = "^[0-9]{2}\\/[0-9]{2}\\/[0-9]{4}$";
    public const string PhoneNumberPattern = "^\\+?[0-9 ]{0,32}$";
    public const string ReferencePattern = "^[0-9a-fA-F]{1,33}$";
    
    
    public const string CallDateFormat = "dd/MM/yyyy";
    public const string EndTimeFormat = "HH:mm:ss";
    
    public Phone CallerId { get; set; }
    public Phone Recipient { get; set; }
    public Date CallDate { get; set; }
    public Time EndTime { get; set; }
    public Span Duration { get; set; }
    public Money Cost { get; set; }
    public CdrReference Reference { get; set; }
    public CdrCallTypeEnum? Type { get; set; }

    public static CdrItem? FromOrNull(ICdrItemEntity? entity)
    {
        return entity != null ? From(entity) : null;
    }
    
    public static CdrItem From(ICdrItemEntity entity)
    {
        return  new CdrItem()
        {
            CallerId = new Phone(entity.CallerId),
            Recipient = new Phone(entity.Recipient),
            CallDate = new Date(entity.CallDate),
            EndTime = new Time(entity.EndTime),
            Duration = new Span(entity.Duration),
            Cost = new Money(entity.Cost, entity.Currency),
            Reference = new CdrReference(entity.Reference),
            Type = (CdrCallTypeEnum?)entity.Type, // TODO: improve
        };
    }
}