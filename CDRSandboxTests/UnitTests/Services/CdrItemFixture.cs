using CDRSandbox.Repositories.ClickHouse.Entities;
using CDRSandbox.Repositories.Interfaces;
using CDRSandbox.Services.Models;
using CDRSandboxTests.Base;
using NUnit.Framework;

namespace CDRSandboxTests.UnitTests.Services;

[TestFixture]
public class CdrItemFixture : RandomDataGeneratorsBase
{
    private static bool CdrItemEqualsICdrItemEntity(CdrItem cdrItem, ICdrItemEntity entity)
    {
        return cdrItem.CallerId.ToString() == entity.CallerId &&
               cdrItem.Recipient.ToString() == entity.Recipient &&
               cdrItem.CallDate == entity.CallDate.Date &&
               cdrItem.EndTime.ToString() == entity.EndTime &&
               cdrItem.Duration.TotalSeconds == entity.Duration &&
               cdrItem.EndTime.ToString() == entity.EndTime &&
               Math.Abs(cdrItem.Cost.Amount - entity.Cost) < 0.001f && // On float we need to use a tolerance for comparison
               cdrItem.Cost.Currency.ToString() == entity.Currency &&
               cdrItem.Reference.ToString() == entity.Reference &&
               cdrItem.Type == (CdrCallTypeEnum?)entity.Type;
    }
    
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
        Assert.That(result, Is.EqualTo(item).Using<CdrItem, ICdrItemEntity>(CdrItemEqualsICdrItemEntity),
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
        Assert.That(result, Is.EqualTo(item).Using<CdrItem, ICdrItemEntity>(CdrItemEqualsICdrItemEntity),
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
        Assert.That(result, Is.EqualTo(item).Using<CdrItem, ICdrItemEntity>(CdrItemEqualsICdrItemEntity),
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
        Assert.That(result, Is.EqualTo(item).Using<CdrItem, ICdrItemEntity>(CdrItemEqualsICdrItemEntity),
            "Items content must be equal");
    }
}