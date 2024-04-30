using CDRSandbox.Services.Models;
using CDRSandbox.Services.Models.ValueObjects;
using CDRSandboxTests.Base;
using NUnit.Framework;

namespace CDRSandboxTests.UnitTests.Services.ValueObjects;

[TestFixture]
public class DateFixture : RandomDataGeneratorsBase
{
    [Test]
    public void Creation_Date_DateTimeValid()
    {
        // Act
        // Assert
        for (int i = 0; i < 10000; i++) // let's test 10000 random values just to make sure
        {
            var date = RandomDateTime;
            Assert.DoesNotThrow(() => new Date(date),
                "Creation with valid pattern must not throw. Error date [{0}]", date);
        }
    }
    
    [Test]
    public void Creation_Date_StringValid()
    {
        // Act
        // Assert
        for (int i = 0; i < 10000; i++) // let's test 10000 random values just to make sure
        {
            var date = RandomDateTime;
            Assert.DoesNotThrow(() => new Date(date.ToString(CdrItem.CallDateFormat)),
                "Creation with valid pattern must not throw. Error date [{0}]", date);
        }
    }
    
    [Test]
    public void Creation_Date_DateOnlyValid()
    {
        // Act
        // Assert
        for (int i = 0; i < 10000; i++) // let's test 10000 random values just to make sure
        {
            var date = RandomDateTime;
            Assert.DoesNotThrow(() => new Date(DateOnly.FromDateTime(date)),
                "Creation with valid pattern must not throw. Error date [{0}]", date);
        }
    }
    
    public void Creation_DateNull()
    {
        // Act
        // Assert
        Assert.Throws<Exception>(() => new Date(null!), "Must throw with null date");
    }
    
    [Test]
    public void Creation_Date_StringInvalid()
    {
        // Act
        // Assert
        Assert.Throws<Exception>(() => new Date("ZZZ"), "Must throw with invalid date");
    }
    
    [Test]
    public void Equality()
    {
        // Arrange
        var date = RandomDateTime;
        var one = new Date(date);
        var two = new Date(date);
        
        // Act
        // Assert
        Assert.That(one == two, Is.True, "Must be equal");
        Assert.That(one.Equals(two), Is.True,"Must be equal");
        Assert.That(Equals(one, two), Is.True,"Must be equal");
        Assert.That(EqualityComparer<Date>.Default.Equals(two, two), Is.True,"Must be equal");
        Assert.That(one, Is.EqualTo(two), "Must be equal");
    }
    
    [Test]
    public void Equality_DateTime()
    {
        // Arrange
        var date = RandomDateTime;
        var one = new Date(date);
        
        // Act
        // Assert
        Assert.That(one == date, Is.True, "Must be equal");
    }
    
    [Test]
    public void Inequality()
    {
        // Arrange
        var firstRand = RandomDateTime;
        var one = new Date(firstRand);
        var secondRand = GenerateDistinctRandom(() => RandomDateTime, [firstRand]);
        var two = new Date(secondRand);
        
        // Act
        // Assert
        Assert.That(one != two, Is.True, "Must be different");
    }
    
    [Test]
    public void Inequality_DateTime()
    {
        // Arrange
        var date = RandomDateTime;
        var secondRand = GenerateDistinctRandom(() => RandomDateTime, [date]);
        var two = new Date(secondRand);
        
        // Act
        // Assert
        Assert.That(two != date, Is.True, "Must be different");
    }
    
    [Test]
    public void ToStringMethod()
    {
        // Arrange
        var date = RandomDateTime;
        
        // Act
        // Assert
        Assert.That(new Date(date).ToString(), Is.EqualTo(date.ToString(CdrItem.CallDateFormat)), "Must be equal");
    }
    
    [Test]
    public void ToDateTime()
    {
        // Arrange
        var date = RandomDateTime;
        
        // Act
        // Assert
        Assert.That(new Date(date).ToDateTime(), Is.EqualTo(date.Date), "Must be equal");
    }
}