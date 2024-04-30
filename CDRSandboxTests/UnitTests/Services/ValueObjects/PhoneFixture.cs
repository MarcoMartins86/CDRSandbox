using CDRSandbox.Services.Models.ValueObjects;
using CDRSandboxTests.Base;
using NUnit.Framework;

namespace CDRSandboxTests.UnitTests.Services.ValueObjects;

[TestFixture]
public class PhoneFixture : RandomDataGeneratorsBase
{
    [Test]
    public void Creation_PhoneValid()
    {
        // Act
        // Assert
        for (int i = 0; i < 10000; i++) // let's test 10000 random values just to make sure
        {
            var phone = RandomPhoneNumber;
            Assert.DoesNotThrow(() => new Phone(phone),
                "Creation with valid pattern must not throw. Error phone [{0}]", phone);
        }
    }
    
    [Test]
    public void Creation_PhoneNull()
    {
        // Act
        // Assert
        Assert.Throws<Exception>(() => new Phone(null!), "Must throw with null phone");
    }
    
    [Test]
    public void Creation_PhoneInvalid()
    {
        // Act
        // Assert
        Assert.Throws<Exception>(() => new Phone("ZZZ"), "Must throw with invalid phone");
    }
    
    [Test]
    public void Equality()
    {
        // Arrange
        var phone = RandomPhoneNumber;
        var one = new Phone(phone);
        var two = new Phone(phone);
        
        // Act
        // Assert
        Assert.That(one == two, Is.True, "Must be equal");
        Assert.That(one.Equals(two), Is.True,"Must be equal");
        Assert.That(Equals(one, two), Is.True,"Must be equal");
        Assert.That(EqualityComparer<Phone>.Default.Equals(two, two), Is.True,"Must be equal");
        Assert.That(one, Is.EqualTo(two), "Must be equal");
    }
    
    [Test]
    public void Inequality()
    {
        // Arrange
        var firstRand = RandomPhoneNumber;
        var one = new Phone(firstRand);
        var secondRand = GenerateDistinctRandom(() => RandomPhoneNumber, [firstRand]);
        var two = new Phone(secondRand);
        
        // Act
        // Assert
        Assert.That(one != two, Is.True, "Must be different");
    }
    
    [Test]
    public void ToStringMethod()
    {
        // Arrange
        var phone = RandomPhoneNumber;
        
        // Act
        // Assert
        Assert.That(new Phone(phone).ToString(), Is.EqualTo(phone), "Must be equal");
    }
}