using CDRSandbox.Controllers.Dtos;
using CDRSandbox.Services.Models;
using CDRSandbox.Services.Models.ValueObjects;
using CDRSandboxTests.Base;
using CDRSandboxTests.Helpers;
using NUnit.Framework;

namespace CDRSandboxTests.UnitTests.Controllers.Dtos;

[TestFixture]
public class CdrItemDtoFixture : RandomDataGeneratorsBase
{
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
            Reference = new Reference(RandomReference),
            Type = RandomType
        };
        
        // Act
        var result = CdrItemDto.FromOrNull(item);
        
        // Assert
        Assert.That(result, Is.EqualTo(item).Using<CdrItemDto, CdrItem>(Comparators.CdrItemDtoEqualsCdrItem),
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
            Reference = new Reference(RandomReference),
            Type = null
        };
        
        // Act
        var result = CdrItemDto.FromOrNull(item);
        
        // Assert
        Assert.That(result, Is.EqualTo(item).Using<CdrItemDto, CdrItem>(Comparators.CdrItemDtoEqualsCdrItem),
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
            Reference = new Reference(RandomReference),
            Type = RandomType
        };
        
        // Act
        var result = CdrItemDto.From(item);
        
        // Assert
        Assert.That(result, Is.EqualTo(item).Using<CdrItemDto, CdrItem>(Comparators.CdrItemDtoEqualsCdrItem),
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
            Reference = new Reference(RandomReference),
            Type = null
        };
        
        // Act
        var result = CdrItemDto.From(item);
        
        // Assert
        Assert.That(result, Is.EqualTo(item).Using<CdrItemDto, CdrItem>(Comparators.CdrItemDtoEqualsCdrItem),
            "Items content must be equal");
    }
}