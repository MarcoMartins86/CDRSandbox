using CDRSandbox.Services.Models.ValueObjects;
using CDRSandboxTests.Base;
using NUnit.Framework;

namespace CDRSandboxTests.UnitTests.Services.ValueObjects;

[TestFixture]
public class SpanFixture : RandomDataGeneratorsBase
{
    [Test]
    public void Creation_SpanValid()
    {
        // Act
        // Assert
        for (int i = 0; i < 10000; i++) // let's test 10000 random values just to make sure
        {
            var span = RandomDuration;
            Assert.DoesNotThrow(() => new Span(span),
                "Creation with valid pattern must not throw. Error span [{0}]", span);
        }
    }
    
    [Test]
    public void Equality()
    {
        // Arrange
        var span = RandomDuration;
        var one = new Span(span);
        var two = new Span(span);
        
        // Act
        // Assert
        Assert.That(one == two, Is.True, "Must be equal");
        Assert.That(one.Equals(two), Is.True,"Must be equal");
        Assert.That(Equals(one, two), Is.True,"Must be equal");
        Assert.That(EqualityComparer<Span>.Default.Equals(two, two), Is.True,"Must be equal");
        Assert.That(one, Is.EqualTo(two), "Must be equal");
    }
    
    [Test]
    public void Inequality()
    {
        // Arrange
        var one = new Span(RandomDuration);
        var two = new Span(RandomDuration);
        
        // Act
        // Assert
        Assert.That(one != two, Is.True, "Must be different");
    }
    
    [Test]
    public void ToStringMethod()
    {
        // Arrange
        var span = RandomDuration;
        
        // Act
        // Assert
        Assert.That(new Span(span).ToString(), Is.EqualTo(span.ToString()), "Must be equal");
    }
    
    [Test]
    public void TotalSeconds()
    {
        // Arrange
        var span = RandomDuration;
        
        // Act
        // Assert
        Assert.That(new Span(span).TotalSeconds, Is.EqualTo(span), "Must be equal");
    }
}