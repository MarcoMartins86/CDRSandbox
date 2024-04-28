using CDRSandbox.Services.Models.ValueObjects;
using CDRSandboxTests.Base;
using NUnit.Framework;

namespace CDRSandboxTests.UnitTests.Services.ValueObjects;

[TestFixture]
public class CdrReferenceFixture : RandomDataGeneratorsBase
{
    [Test]
    public void Creation_ReferenceValid()
    {
        // Act
        // Assert
        for (int i = 0; i < 10000; i++) // let's test 10000 random values just to make sure
        {
            var reference = RandomReference;
            Assert.DoesNotThrow(() => new CdrReference(reference),
                "Creation with valid pattern must not throw. Error reference [{0}]", reference);
        }
    }
    
    [Test]
    public void Creation_ReferenceNull()
    {
        // Act
        // Assert
        Assert.Throws<Exception>(() => new CdrReference(null!), "Must throw with null reference");
    }
    
    [Test]
    public void Creation_ReferenceInvalid()
    {
        // Act
        // Assert
        Assert.Throws<Exception>(() => new CdrReference("LHOIBP"), "Must throw with invalid reference");
    }
    
    [Test]
    public void Creation_ReferenceWhiteSpaces()
    {
        // Act
        // Assert
        Assert.Throws<Exception>(() => new CdrReference("      "), "Must throw with only spaces");
    }
    
    [Test]
    public void Equality()
    {
        // Arrange
        var reference = RandomReference;
        var one = new CdrReference(reference);
        var two = new CdrReference(reference);
        
        // Act
        // Assert
        Assert.That(one == two, Is.True, "Must be equal");
        Assert.That(one.Equals(two), Is.True,"Must be equal");
        Assert.That(Equals(one, two), Is.True,"Must be equal");
        Assert.That(EqualityComparer<CdrReference>.Default.Equals(two, two), Is.True,"Must be equal");
        Assert.That(one, Is.EqualTo(two), "Must be equal");
    }
    
    [Test]
    public void Inequality()
    {
        // Arrange
        var one = new CdrReference(RandomReference);
        var two = new CdrReference(RandomReference);
        
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
        Assert.That(new CdrReference(reference).ToString(), Is.EqualTo(reference), "Must be equal");
    }
}