﻿using CDRSandbox.Services.Models.ValueObjects;
using CDRSandboxTests.Base;
using NUnit.Framework;

namespace CDRSandboxTests.UnitTests.Services.ValueObjects;

[TestFixture]
public class MoneyFixture : RandomDataGeneratorsBase
{
    [Test]
    public void Creation_MoneyValid()
    {
        // Act
        // Assert
        for (int i = 0; i < 10000; i++) // let's test 10000 random values just to make sure
        {
            var currency = RandomCurrency;
            var cost = RandomCost;
            Assert.DoesNotThrow(() => new Money(cost, currency),
                "Creation with valid pattern must not throw. Error money [{0}][{1}]", cost, currency);
        }
    }
    
    [Test]
    public void Creation_Money_CurrencyNull()
    {
        // Act
        // Assert
        Assert.Throws<Exception>(() => new Money(RandomCost, null!), "Must throw with null currency");
    }
    
    [Test]
    public void Creation_Money_CurrencyInvalid()
    {
        // Act
        // Assert
        Assert.Throws<Exception>(() => new Money(RandomCost, "ZZZ"), "Must throw with invalid currency");
    }
    
    [Test]
    public void Equality()
    {
        // Arrange
        var currency = RandomCurrency;
        var cost = RandomCost;
        var one = new Money(cost, currency);
        var two = new Money(cost, currency);
        
        // Act
        // Assert
        Assert.That(one == two, Is.True, "Must be equal");
        Assert.That(one.Equals(two), Is.True,"Must be equal");
        Assert.That(Equals(one, two), Is.True,"Must be equal");
        Assert.That(EqualityComparer<Money>.Default.Equals(two, two), Is.True,"Must be equal");
        Assert.That(one, Is.EqualTo(two), "Must be equal");
    }
    
    [Test]
    public void Inequality()
    {
        // Arrange
        var firstRandCost = RandomCost;
        var firstRandCurrency = RandomCurrency;
        var one = new Money(firstRandCost, firstRandCurrency);
        var secondRandCost = GenerateDistinctRandom(() => RandomCost, [firstRandCost]);
        var secondRandCurrency = GenerateDistinctRandom(() => RandomCurrency, [firstRandCurrency]);
        var two = new Money(secondRandCost, secondRandCurrency);
        
        // Act
        // Assert
        Assert.That(one != two, Is.True, "Must be different");
    }
    
    [Test]
    public void Inequality_SameCost()
    {
        // Arrange
        var cost = RandomCost;
        var firstRand = RandomCurrency;
        var one = new Money(cost, firstRand);
        var secondRand = GenerateDistinctRandom(() => RandomCurrency, [firstRand]);
        var two = new Money(cost, secondRand);
        
        // Act
        // Assert
        Assert.That(one != two, Is.True, "Must be different");
    }
    
    [Test]
    public void Inequality_SameCurrency()
    {
        // Arrange
        var currency = RandomCurrency;
        var firstRand = RandomCost;
        var one = new Money(firstRand, currency);
        var secondRand = GenerateDistinctRandom(() => RandomCost, [firstRand]);
        var two = new Money(secondRand, currency);
        
        // Act
        // Assert
        Assert.That(one != two, Is.True, "Must be different");
    }
    
    [Test]
    public void ToStringMethod()
    {
        // Arrange
        var currency = RandomCurrency;
        var cost = Math.Round(RandomCost, 3, MidpointRounding.ToEven);
        
        // Act
        // Assert
        Assert.That(new Money(cost, currency).ToString(), Is.EqualTo($"{cost} {currency}"), "Must be equal");
    }
}