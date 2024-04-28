using CDRSandbox.Controllers.Dto;
using CDRSandbox.Services.Models;
using CDRSandbox.Services.Models.ValueObjects;
using CDRSandboxTests.Base;
using NUnit.Framework;

namespace CDRSandboxTests.UnitTests.Controllers;

[TestFixture]
public class CdrItemDtoFixture : RandomDataGeneratorsBase
{
    private static bool CdrItemDtoEqualsCdrItem(CdrItemDto dto, CdrItem cdrItem)
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
    
    [Test]
    public void FromOrNull_CdrItemIsNull_ReturnsNull()
    {
        // Act
        var result = CdrItemDto.FromOrNull(null);
        
        // Assert
        Assert.That(result, Is.Null, "Result must be null");
    }
    
    [Test]
    public void FromOrNull_CdrItemWithType_ReturnsCorrectValues()
    {
        // Arrange
        var item = new CdrItem()
        {
            CallerId = new Phone(RandomPhoneNumber),
            Recipient = new Phone(RandomPhoneNumber),
            CallDate = new Date(RandomDateTime),
            EndTime = new Time(RandomTime),
            Duration = new Span(RandomDuration),
            Cost = new Money(RandomCost, RandomCurrency),
            Reference = new CdrReference(RandomReference),
            Type = RandomType
        };
        
        // Act
        var result = CdrItemDto.FromOrNull(item);
        
        // Assert
        Assert.That(result, Is.EqualTo(item).Using<CdrItemDto, CdrItem>(CdrItemDtoEqualsCdrItem),
            "Items content must be equal");
    }
    
    [Test]
    public void FromOrNull_CdrItemWithNullType_ReturnsCorrectValues()
    {
        // Arrange
        var item = new CdrItem()
        {
            CallerId = new Phone(RandomPhoneNumber),
            Recipient = new Phone(RandomPhoneNumber),
            CallDate = new Date(RandomDateTime),
            EndTime = new Time(RandomTime),
            Duration = new Span(RandomDuration),
            Cost = new Money(RandomCost, RandomCurrency),
            Reference = new CdrReference(RandomReference),
            Type = null
        };
        
        // Act
        var result = CdrItemDto.FromOrNull(item);
        
        // Assert
        Assert.That(result, Is.EqualTo(item).Using<CdrItemDto, CdrItem>(CdrItemDtoEqualsCdrItem),
            "Items content must be equal");
    }
    
    [Test]
    public void From_CdrItemIsNull_Throws()
    {
        // Act
        // Assert
        Assert.Throws<NullReferenceException>(() => CdrItemDto.From(null!), "Must throw NullReferenceException");
    }
    
    [Test]
    public void From_CdrItemWithType_ReturnsCorrectValues()
    {
        // Arrange
        var item = new CdrItem()
        {
            CallerId = new Phone(RandomPhoneNumber),
            Recipient = new Phone(RandomPhoneNumber),
            CallDate = new Date(RandomDateTime),
            EndTime = new Time(RandomTime),
            Duration = new Span(RandomDuration),
            Cost = new Money(RandomCost, RandomCurrency),
            Reference = new CdrReference(RandomReference),
            Type = RandomType
        };
        
        // Act
        var result = CdrItemDto.From(item);
        
        // Assert
        Assert.That(result, Is.EqualTo(item).Using<CdrItemDto, CdrItem>(CdrItemDtoEqualsCdrItem),
            "Items content must be equal");
    }
    
    [Test]
    public void From_CdrItemWithNullType_ReturnsCorrectValues()
    {
        // Arrange
        var item = new CdrItem()
        {
            CallerId = new Phone(RandomPhoneNumber),
            Recipient = new Phone(RandomPhoneNumber),
            CallDate = new Date(RandomDateTime),
            EndTime = new Time(RandomTime),
            Duration = new Span(RandomDuration),
            Cost = new Money(RandomCost, RandomCurrency),
            Reference = new CdrReference(RandomReference),
            Type = null
        };
        
        // Act
        var result = CdrItemDto.From(item);
        
        // Assert
        Assert.That(result, Is.EqualTo(item).Using<CdrItemDto, CdrItem>(CdrItemDtoEqualsCdrItem),
            "Items content must be equal");
    }
}