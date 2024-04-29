using CDRSandbox.Controllers.Dtos;
using CDRSandbox.Repositories.Interfaces;
using CDRSandbox.Services.Models;

namespace CDRSandboxTests.Helpers;

public static class Comparators
{
    public static bool CdrItemDtoEqualsCdrItem(CdrItemDto dto, CdrItem cdrItem)
    {
        return dto.CallerId == cdrItem.CallerId.ToString() &&
               dto.Recipient == cdrItem.Recipient.ToString() &&
               dto.CallDate == cdrItem.CallDate.ToString() &&
               dto.EndTime == cdrItem.EndTime.ToString() &&
               dto.Duration == cdrItem.Duration.TotalSeconds &&
               dto.EndTime == cdrItem.EndTime.ToString() &&
               Math.Abs(dto.Cost - cdrItem.Cost.Amount) < 0.001f && // On float we need to use a tolerance for comparison
               dto.Currency == cdrItem.Cost.Currency.ToString() &&
               dto.Reference == cdrItem.Reference.ToString() &&
               dto.Type == cdrItem.Type;
    }
    
    public static bool CdrItemDtoEqualsCdrCsvItem(CdrItemDto dto, CdrCsvItem cdrItem)
    {
        return dto.CallerId == cdrItem.CallerId &&
               dto.Recipient == cdrItem.Recipient &&
               DateOnly.ParseExact(dto.CallDate, CdrItem.CallDateFormat)  == cdrItem.CallDate &&
               TimeOnly.ParseExact(dto.EndTime, CdrItem.EndTimeFormat) == cdrItem.EndTime &&
               dto.Duration == cdrItem.Duration &&
               cdrItem.Cost != null && Math.Abs(dto.Cost - (float)cdrItem.Cost) < 0.001f && // On float we need to use a tolerance for comparison
               dto.Currency == cdrItem.Currency &&
               dto.Reference == cdrItem.Reference &&
               dto.Type == cdrItem.Type;
    }
    
    public static bool CdrItemEqualsICdrItemEntity(CdrItem cdrItem, ICdrItemEntity entity)
    {
        return cdrItem.CallerId.ToString() == entity.CallerId &&
               cdrItem.Recipient.ToString() == entity.Recipient &&
               cdrItem.CallDate == entity.CallDate.Date &&
               cdrItem.EndTime.ToString() == entity.EndTime &&
               cdrItem.Duration.TotalSeconds == entity.Duration &&
               Math.Abs(cdrItem.Cost.Amount - entity.Cost) < 0.001f && // On float we need to use a tolerance for comparison
               cdrItem.Cost.Currency.ToString() == entity.Currency &&
               cdrItem.Reference.ToString() == entity.Reference &&
               cdrItem.Type == (CdrCallTypeEnum?)entity.Type;
    }
}