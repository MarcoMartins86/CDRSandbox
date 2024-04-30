using CDRSandbox.Services.Models.ValueObjects;
using CDRSandboxTests.Base;
using NUnit.Framework;

namespace CDRSandboxTests.UnitTests.Services.ValueObjects;

[TestFixture]
public class ReferenceFixture : RandomDataGeneratorsBase
{
    [Test]
    public void Creation_ReferenceValid()
    {
        // Act
        // Assert
        for (int i = 0; i < 10000; i++) // let's test 10000 random values just to make sure
        {
            var reference = RandomReference;
            Assert.DoesNotThrow(() => new Reference(reference),
                "Creation with valid pattern must not throw. Error reference [{0}]", reference);
        }
    }
    
    [Test]
    public void Creation_ReferenceNull()
    {
        // Act
        // Assert
        Assert.Throws<Exception>(() => new Reference(null!), "Must throw with null reference");
    }
    
    [Test]
    public void Creation_ReferenceInvalid()
    {
        // Act
        // Assert
        Assert.Throws<Exception>(() => new Reference("LHOIBP"), "Must throw with invalid reference");
    }
    
    [Test]
    public void Creation_ReferenceWhiteSpaces()
    {
        // Act
        // Assert
        Assert.Throws<Exception>(() => new Reference("      "), "Must throw with only spaces");
    }
    
    [Test]
    public void Equality()
    {
        // Arrange
        var reference = RandomReference;
        var one = new Reference(reference);
        var two = new Reference(reference);
        
        // Act
        // Assert
        Assert.That(one == two, Is.True, "Must be equal");
        Assert.That(one.Equals(two), Is.True,"Must be equal");
        Assert.That(Equals(one, two), Is.True,"Must be equal");
        Assert.That(EqualityComparer<Reference>.Default.Equals(two, two), Is.True,"Must be equal");
        Assert.That(one, Is.EqualTo(two), "Must be equal");
    }
    
    [Test]
    public void Inequality()
    {
        // Arrange
        var firstRand = RandomReference;
        var one = new Reference(firstRand);
        var secondRand = GenerateDistinctRandom(() => RandomReference, [firstRand]);
        var two = new Reference(secondRand);
        
        // Act
        // Assert
        Assert.That(one != two, Is.True, "Must be different");
    }
    
    [Test]
    public void ToStringMethod()
    {
        // Arrange
        var reference = RandomReference;
        
        // Act
        // Assert
        Assert.That(new Reference(reference).ToString(), Is.EqualTo(reference), "Must be equal");
    }
}