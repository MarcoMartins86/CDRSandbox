using CDRSandbox.Repositories.ClickHouse.Entities;
using CDRSandbox.Repositories.Interfaces;
using CDRSandbox.Services.Models;
using CDRSandboxTests.Base;
using CDRSandboxTests.Helpers;
using NUnit.Framework;

namespace CDRSandboxTests.UnitTests.Services;

[TestFixture]
public class CdrItemFixture : RandomDataGeneratorsBase
{
    [Test]
    public void FromOrNull_ICdrItemEntityIsNull_ReturnsNull()
    {
        // Act
        var result = CdrItem.FromOrNull(null);
        
        // Assert
        Assert.That(result, Is.Null, "Result must be null");
    }
    
    [Test]
    public void FromOrNull_CdrItemClickHouseEntityWithType_ReturnsCorrectValues()
    {
        // Arrange
        var item = new CdrItemClickHouseEntity()
        {
            CallerId = RandomPhoneNumber,
            Recipient = RandomPhoneNumber,
            CallDate = RandomDateTime.Date,
            EndTime = RandomTime,
            Duration = RandomDuration,
            Cost = RandomCost,
            Currency = RandomCurrency,
            Reference = RandomReference,
            Type = (int)RandomType
        };
        
        // Act
        var result = CdrItem.FromOrNull(item);
        
        // Assert
        Assert.That(result, Is.EqualTo(item).Using<CdrItem, ICdrItemEntity>(Comparators.CdrItemEqualsICdrItemEntity),
            "Items content must be equal");
    }
    
    [Test]
    public void FromOrNull_CdrItemClickHouseEntityWithNullType_ReturnsCorrectValues()
    {
        // Arrange
        var item = new CdrItemClickHouseEntity()
        {
            CallerId = RandomPhoneNumber,
            Recipient = RandomPhoneNumber,
            CallDate = RandomDateTime.Date,
            EndTime = RandomTime,
            Duration = RandomDuration,
            Cost = RandomCost,
            Currency = RandomCurrency,
            Reference = RandomReference,
            Type = null
        };
        
        // Act
        var result = CdrItem.FromOrNull(item);
        
        // Assert
        Assert.That(result, Is.EqualTo(item).Using<CdrItem, ICdrItemEntity>(Comparators.CdrItemEqualsICdrItemEntity),
            "Items content must be equal");
    }
    
    [Test]
    public void From_ICdrItemEntityIsNull_Throws()
    {
        // Act
        // Assert
        Assert.Throws<NullReferenceException>(() => CdrItem.From(null!), "Must throw NullReferenceException");
    }
    
    [Test]
    public void From_CdrItemClickHouseEntityWithType_ReturnsCorrectValues()
    {
        // Arrange
        var item = new CdrItemClickHouseEntity()
        {
            CallerId = RandomPhoneNumber,
            Recipient = RandomPhoneNumber,
            CallDate = RandomDateTime.Date,
            EndTime = RandomTime,
            Duration = RandomDuration,
            Cost = RandomCost,
            Currency = RandomCurrency,
            Reference = RandomReference,
            Type = (int)RandomType
        };
        
        // Act
        var result = CdrItem.From(item);
        
        // Assert
        Assert.That(result, Is.EqualTo(item).Using<CdrItem, ICdrItemEntity>(Comparators.CdrItemEqualsICdrItemEntity),
            "Items content must be equal");
    }
    
    [Test]
    public void From_CdrItemWithNullType_ReturnsCorrectValues()
    {
        // Arrange
        var item = new CdrItemClickHouseEntity()
        {
            CallerId = RandomPhoneNumber,
            Recipient = RandomPhoneNumber,
            CallDate = RandomDateTime.Date,
            EndTime = RandomTime,
            Duration = RandomDuration,
            Cost = RandomCost,
            Currency = RandomCurrency,
            Reference = RandomReference,
            Type = null
        };
        
        // Act
        var result = CdrItem.From(item);
        
        // Assert
        Assert.That(result, Is.EqualTo(item).Using<CdrItem, ICdrItemEntity>(Comparators.CdrItemEqualsICdrItemEntity),
            "Items content must be equal");
    }
}