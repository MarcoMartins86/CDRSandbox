using CDRSandbox.Services.Models.ValueObjects;
using CDRSandboxTests.Base;
using NUnit.Framework;

namespace CDRSandboxTests.UnitTests.Services.ValueObjects;

[TestFixture]
public class CurrencyFixture : RandomDataGeneratorsBase
{
    [Test]
    public void Creation_CurrencyValid()
    {
        // Act
        // Assert
        for (int i = 0; i < 10000; i++) // let's test 10000 random values just to make sure
        {
            var currency = RandomCurrency;
            Assert.DoesNotThrow(() => new Currency(currency),
                "Creation with valid pattern must not throw. Error currency [{0}]", currency);
        }
    }
    
    [Test]
    public void Creation_CurrencyNull()
    {
        // Act
        // Assert
        Assert.Throws<Exception>(() => new Currency(null!), "Must throw with null currency");
    }
    
    [Test]
    public void Creation_CurrencyInvalid()
    {
        // Act
        // Assert
        Assert.Throws<Exception>(() => new Currency("ZZZ"), "Must throw with invalid currency");
    }
    
    [Test]
    public void Equality()
    {
        // Arrange
        var currency = RandomCurrency;
        var one = new Currency(currency);
        var two = new Currency(currency);
        
        // Act
        // Assert
        Assert.That(one == two, Is.True, "Must be equal");
        Assert.That(one.Equals(two), Is.True,"Must be equal");
        Assert.That(Equals(one, two), Is.True,"Must be equal");
        Assert.That(EqualityComparer<Currency>.Default.Equals(two, two), Is.True,"Must be equal");
        Assert.That(one, Is.EqualTo(two), "Must be equal");
    }
    
    [Test]
    public void Inequality()
    {
        // Arrange
        var one = new Currency(Currency.ActiveCurrencyArray[0]);
        var two = new Currency(Currency.ActiveCurrencyArray[1]);
        
        // Act
        // Assert
        Assert.That(one != two, Is.True, "Must be different");
    }
    
    [Test]
    public void ToStringMethod()
    {
        // Arrange
        var currency = RandomCurrency;
        
        // Act
        // Assert
        Assert.That(new Currency(currency).ToString(), Is.EqualTo(currency), "Must be equal");
    }
}